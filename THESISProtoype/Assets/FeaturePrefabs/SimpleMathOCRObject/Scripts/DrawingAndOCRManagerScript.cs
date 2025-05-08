// Source: https://www.youtube.com/watch?v=qOP83fot3c0

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Sentis;
using System.Linq;

public class DrawingAndOCRManagerScript : MonoBehaviour
{

    public Camera cam;//Reference to the camera in the scene

    //Canvas dimensions
    public int totalXPixels = 45;
    public int totalYPixels = 45;

    //Brush properties
    public int brushSize = 4;
    public Color brushColor;

    //Wether the drawing system will use interpolation to make smoother lines(will affect the performance)
    public bool useInterpolation = true;

    //References to the points on our drawable face
    public Transform topLeftCorner;
    public Transform bottomRightCorner;
    public Transform point;

    //Reference to the material which will use this texture (This one is the OCRPlaneMateiral)
    public Material material;

    //The generated texture
    public Texture2D generatedTexture;

    //The array which contains the color of the pixels
    Color[] colorMap;

    //The current coordinates of the cursor in the current frame
    int xPixel = 0;
    int yPixel = 0;

    //Variables necessary for interpolation
    bool pressedLastFrame = false;//This bool remembers wether we click over the drawable area in the last frame
    int lastX = 0;//These variables remember the coordinates of the cursor in the last frame
    int lastY = 0;

    //These variables hold constants which are precalculated in order to save performance
    float xMult;
    float yMult;

    //Variables that determines how long until the drawings are submitted to OCR model and cleared, also related ones
    public float CLEAR_TIME = 0.75f;
    private float timer = 0f;
    private bool clear = false;
    private bool hasDrawn = false; //Prevents the OCR from starting before drawing
    private bool processing = false; //Stops drawing if needed

    //Variable for OCR model in .onnx and the model and worker vars
    public ModelAsset OCRModel;
    private Model runtimeModel;
    private Worker worker;

    //Variables for dealing with the VFX drawing layer
    public GameObject vfxLineGO;
    private LineRenderer line; //Store LR component of above on Start()
    private Vector3 previousPosition, currentPosition; //Variables for VFX line positions
    public float vfxLineMinDistance = 0.1f;
    private List<GameObject> vfxLineClones;

    //Local raycast variables turned into fields
    private Ray ray;
    private RaycastHit hit;
    public float raycast_range = 20f; //Range of raycast, default 20f

    //Reference to material for line renderer
    public Material vfxLineMaterial;

    //Public array containing output classes
    public readonly string[] CLASSES = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "Lpar", "Rpar", "dec", "div", "equ", "min", "pi", "plu", "tim" };

    //Boolean for saving OCR inputs as .jpg, make sure this is false before deploying
    public bool DO_SAVE_INPUTS = false;

    //Path to OCR input image
    private string path;

    private void Start()
    {
        //Initializing the colorMap array with width * height elements
        colorMap = new Color[totalXPixels * totalYPixels];
        generatedTexture = new Texture2D(totalYPixels, totalXPixels, TextureFormat.RGBA32, false); //Generating a new texture with width and height
        generatedTexture.filterMode = FilterMode.Point;
        material.SetTexture("_BaseMap", generatedTexture); //Giving our material the new texture

        ResetColor(); //Resetting the color of the canvas to white

        xMult = totalXPixels / (bottomRightCorner.localPosition.x - topLeftCorner.localPosition.x);//Precalculating constants
        yMult = totalYPixels / (bottomRightCorner.localPosition.y - topLeftCorner.localPosition.y);

        //Init VFXLine variables
        vfxLineGO.GetComponent<LineRenderer>().material = vfxLineMaterial; //Set material here
        vfxLineClones = new List<GameObject>();
        ResetVFX();

        //Init timer
        timer = CLEAR_TIME;

        //Init OCR model
        runtimeModel = ModelLoader.Load(OCRModel);
    }

    private void Update()
    {
        if (processing) //do nothing if OCR is not done
            return;
        
        if(timer > 0f) //Decrease time if not zero
        {
            timer -= Time.deltaTime;
            if(timer <= 0f) //If timer is done set clear to true
            {
                clear = true;
            }
        }    
        
        if (Input.GetMouseButton(0))//If the mouse is pressed, call the function
        {
            //Function for drawing stuff on the OCR input
            CalculatePixel();

            //Draw VFX stuff (line sfx whatever), needs to occur after OCR draw because raycast point needed
            DrawVFX();

            //Set/Reset variables
            hasDrawn = true; //Permanently set to true
            timer = CLEAR_TIME; // Reset timer
            clear = false; // Reset clear flag
        }
        else //Else, we did not draw, so on the next frame we should not apply interpolation
        {
            pressedLastFrame = false;

            //Make new LR object for multiple lines
            MakeNewLine();

            if (clear && hasDrawn)
            {
                RotateImage(generatedTexture, -90f); //Rotate texture first since it is tilted for some reason
                
                //Call OCR model method, this will also pass the output to wherever later
                PerformOCR();

                if (DO_SAVE_INPUTS) //Save OCR input as screenshot if bool is true
                {
                    //Set path to image
                    path = Path.Combine(Application.persistentDataPath, UnityEngine.Random.Range(10000, 9999999999999999999).ToString() + ".jpg");
                    File.WriteAllBytes(path, ImageConversion.EncodeToJPG(generatedTexture)); //Screenshot function
                }

                ResetColor(); //Clear OCR input drawings

                ResetVFX(); //Reset VFX line and whatever effects are added later

                clear = false; // Reset clear flag
            }
        }
    }

    //Function for adding vfx, to avoid clutter on Update()
    private void DrawVFX()
    {
        currentPosition = hit.point; //Draw on raycast hit position

        //NOTE: CHANGE SIGN OF OFFSET DEPENDING ON DIRECTION OF CAMERA
        currentPosition.z = vfxLineGO.transform.position.z + 0.01f; //Spawn line slightly above the OCR and VFX planes

        //Only add new point if distance to new position is greater than min distance
        if(Vector3.Distance(currentPosition, previousPosition) > vfxLineMinDistance)
        {
            if(previousPosition == vfxLineGO.transform.position) //Fix to sudden jump if not starting in center
            {
                line.SetPosition(0, currentPosition); //Set starting point to current position
            }
            else
            {
                line.positionCount++; //Increment num positions
                line.SetPosition(line.positionCount - 1, currentPosition); //Set latest line pos to raycast hit point
            }

            previousPosition = currentPosition; //Set current pos as previous now that it is done being used
        }
    }

    //Anti clutter
    private void MakeNewLine()
    {
        //Clone linerenderer and set as current line if current linepositions > 1, Done to allow multiple lines
        if (line.positionCount > 1)
        {
            vfxLineClones.Add(Instantiate(vfxLineGO));
            line = vfxLineClones.Last().GetComponent<LineRenderer>();

            //Reset new line stuff to defaults
            previousPosition = vfxLineGO.transform.position;
            line.positionCount = 1;
        }
    }

    //Function for clearing vfx, to avoid clutter on Update()
    private void ResetVFX()
    {
        line = vfxLineGO.GetComponent<LineRenderer>(); //Go back to default

        //Delete all clones
        foreach (GameObject go in vfxLineClones) { 
            Destroy(go);
        }

        previousPosition = vfxLineGO.transform.position;
        line.positionCount = 1; //Allow line to start with 1 dot
    }

    void CalculatePixel()//This function checks if the cursor is currently over the canvas and, if it is, it calculates which pixel on the canvas it is on
    {
        ray = cam.ScreenPointToRay(Input.mousePosition);//Get a ray from the center of the camera to the mouse position
        if (Physics.Raycast(ray, out hit, raycast_range))//Check if the ray hits something
        {
            point.position = hit.point;//Move to pointer to the place where the mouse intersects the canvas
            xPixel = (int)((point.localPosition.x - topLeftCorner.localPosition.x) * xMult); //Calculate the position in pixels
            yPixel = (int)((point.localPosition.y - topLeftCorner.localPosition.y) * yMult);
            ChangePixelsAroundPoint(); //Call the next function
        }
        else
            pressedLastFrame = false; //We did not draw, so the next frame we should not apply interpolation
    }

    void ChangePixelsAroundPoint() //This function checks wether interpolation should be applied and if it should, it applies it
    {
        if (useInterpolation && pressedLastFrame && (lastX != xPixel || lastY != yPixel)) //Check if we should use interpolation
        {
            int dist = (int)Mathf.Sqrt((xPixel - lastX) * (xPixel - lastX) + (yPixel - lastY) * (yPixel - lastY)); //Calculate the distance between the current pixel and the pixel from last frame
            for (int i = 1; i <= dist; i++) //Loop through the points on the determined line
                DrawBrush((i * xPixel + (dist - i) * lastX) / dist, (i * yPixel + (dist - i) * lastY) / dist); //Call the DrawBrush method on the determined points
        }
        else //We shouldn't apply interpolation
            DrawBrush(xPixel, yPixel); //Call the DrawBrush method
        pressedLastFrame = true; //We should apply interpolation on the next frame
        lastX = xPixel;
        lastY = yPixel;
        SetTexture();//Updating the texture
    }

    void DrawBrush(int xPix, int yPix) //This function takes a point on the canvas as a parameter and draws a circle with radius brushSize around it
    {
        int i = xPix - brushSize + 1, j = yPix - brushSize + 1, maxi = xPix + brushSize - 1, maxj = yPix + brushSize - 1; //Declaring the limits of the circle
        if (i < 0) //If either lower boundary is less than zero, set it to be zero
            i = 0;
        if (j < 0)
            j = 0;
        if (maxi >= totalXPixels) //If either upper boundary is more than the maximum amount of pixels, set it to be under
            maxi = totalXPixels - 1;
        if (maxj >= totalYPixels)
            maxj = totalYPixels - 1;
        for (int x = i; x <= maxi; x++)//Loop through all of the points on the square that frames the circle of radius brushSize
        {
            for (int y = j; y <= maxj; y++)
            {
                if ((x - xPix) * (x - xPix) + (y - yPix) * (y - yPix) <= brushSize * brushSize) //Using the circle's formula(x^2+y^2<=r^2) we check if the current point is inside the circle
                    colorMap[x * totalYPixels + y] = brushColor;
            }
        }
    }

    void SetTexture() //This function applies the texture
    {
        generatedTexture.SetPixels(colorMap);
        generatedTexture.Apply();
    }

    void ResetColor() //This function resets the color to white
    {
        for (int i = 0; i < colorMap.Length; i++)
            colorMap[i] = Color.white;
        SetTexture();
    }

    // Source: https://gamedev.stackexchange.com/questions/203539/rotating-a-unity-texture2d-90-180-degrees-without-using-getpixels32-or-setpixels
    private void RotateImage(Texture2D tex, float angleDegrees)
    {
        int width = tex.width;
        int height = tex.height;
        float halfHeight = height * 0.5f;
        float halfWidth = width * 0.5f;

        var texels = tex.GetRawTextureData<Color32>();
        var copy = System.Buffers.ArrayPool<Color32>.Shared.Rent(texels.Length);
        Unity.Collections.NativeArray<Color32>.Copy(texels, copy, texels.Length);

        float phi = Mathf.Deg2Rad * angleDegrees;
        float cosPhi = Mathf.Cos(phi);
        float sinPhi = Mathf.Sin(phi);

        int address = 0;
        for (int newY = 0; newY < height; newY++)
        {
            for (int newX = 0; newX < width; newX++)
            {
                float cX = newX - halfWidth;
                float cY = newY - halfHeight;
                int oldX = Mathf.RoundToInt(cosPhi * cX + sinPhi * cY + halfWidth);
                int oldY = Mathf.RoundToInt(-sinPhi * cX + cosPhi * cY + halfHeight);
                bool InsideImageBounds = (oldX > -1) & (oldX < width)
                                       & (oldY > -1) & (oldY < height);

                texels[address++] = InsideImageBounds ? copy[oldY * width + oldX] : default;
            }
        }

        // No need to reinitialize or SetPixels - data is already in-place.
        tex.Apply(true);

        System.Buffers.ArrayPool<Color32>.Shared.Return(copy);
    }

    //To avoid clutter on update()
    private void PerformOCR()
    {
        processing = true; //flag this
        
        // Load input data to tensor
        Tensor<float> inputTensor = new Tensor<float>(new TensorShape(1, 3, 45, 45));
        TextureConverter.ToTensor(generatedTexture, inputTensor, new TextureTransform());

        //Run inference
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
        worker.Schedule(inputTensor);

        //Store output tensor as array
        var outputTensor = worker.PeekOutput() as Tensor<float>;
        var cpuTensor = outputTensor.ReadbackAndClone();
        float[] outputs = cpuTensor.DownloadToArray();

        //Cleanup
        inputTensor.Dispose();
        worker.Dispose();
        outputTensor.Dispose();
        cpuTensor.Dispose();

        //Get Output Class
        float max = outputs.Max();
        int index = System.Array.IndexOf(outputs, max);
        string imgClass = CLASSES[index];

        //Debug Only
        Debug.Log("imgClass: " + imgClass);

        //TODO: Add way to transmit the class output, either through message or reference of target object

        processing = false; //reset flag
    }
}
