using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CHARACTERS;
using COMMANDS;
using UnityEngine;

namespace DIALOGUE
{
    public class ConversationManager
    {
        private VNDialogueSystem dialogueSystem => VNDialogueSystem.instance;
        private Coroutine process = null;
        public bool isRunning = false;
        private TextArchitect architect = null;
        private bool userPrompt = false;

        public ConversationManager(TextArchitect architect)
        {
            this.architect = architect;
            dialogueSystem.onUserPrompt_Next += OnUserPrompt_Next;
        }

        private void OnUserPrompt_Next()
        {
            userPrompt = true;
        }

        public Coroutine StartConversation(List<string> conversation)
        {
            StopConversation();

            process = dialogueSystem.StartCoroutine(RunningConversation(conversation));

            return process;
        }

        public void StopConversation()
        {
            if (!isRunning)
                return;

            dialogueSystem.StopCoroutine(process);
            isRunning = false;
        }

        IEnumerator RunningConversation(List<string> conversation)
        {
            for (int i = 0; i < conversation.Count; i++)
            {
                //Skip blank lines
                if (string.IsNullOrWhiteSpace(conversation[i]))
                    continue;
                DIALOGUE_LINE line = DialogueParser.Parse(conversation[i]);

                // Show Dialogue
                if (line.hasDialogue)
                    yield return Line_RunDialogue(line);

                // Run Commands
                if (line.hasCommands)
                    yield return Line_RunCommands(line);

                if (line.hasDialogue)
                {
                    // Wait for user input
                    yield return WaitForUserInput();
                    // The code below autoplays the next line after 1 second
                    // Remove it if you want to force user input for every line
                    // yield return new WaitForSeconds(1);
                }
            }
        }

        IEnumerator Line_RunDialogue(DIALOGUE_LINE line)
        {
            //Show or hide the speaker if there's one present
            if (line.hasSpeaker)
                HandleSpeakerLogic(line.speakerData);
            // Build the dialogue text
            yield return BuildLineSegments(line.dialogueData);

        }


        private void HandleSpeakerLogic(DL_SPEAKER_DATA speakerData)
        {
            bool CharacterMustBeCreated = (speakerData.makeCharacterEnter || speakerData.isCastingPosition || speakerData.isCastingExpressions);

            Character character = CharacterManager.instance.GetCharacter(speakerData.name, CharacterMustBeCreated);

            if (speakerData.makeCharacterEnter && (!character.isVisible && !character.isRevealing))
            {
                character.Show();
            }

            // Add character name to the UI
            dialogueSystem.ShowSpeakerName(speakerData.displayname);

            // Now customize dialogue for character if applicable
            VNDialogueSystem.instance.ApplySpeakerDataToDialogueContainer(speakerData.name);

            // Cast Position
            if (speakerData.isCastingPosition)
                character.MoveToPosition(speakerData.castPosition); //character.SetPosition(speakerData.castPosition); // instant position change

            // Cast Expressions
            if (speakerData.isCastingExpressions)
            {
                foreach (var ce in speakerData.CastExpressions)
                {
                    character.OnReceiveCastingExpression(ce.layer, ce.expression);
                }
            }
        }

        IEnumerator Line_RunCommands(DIALOGUE_LINE line)
        {
            List<DL_COMMAND_DATA.Command> commands = line.commandData.commands;

            foreach (DL_COMMAND_DATA.Command command in commands)
            {
                if (command.waitForCompletion || command.name == "wait")
                    yield return CommandManager.instance.Execute(command.name, command.arguments);
                else
                    CommandManager.instance.Execute(command.name, command.arguments);
            }
            yield return null;
        }

        IEnumerator BuildLineSegments(DL_DIALOGUE_DATA line)
        {
            for (int i = 0; i < line.segments.Count; i++)
            {
                DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment = line.segments[i];

                yield return WaitForDialogueSegmentSignalToBeTriggered(segment);

                yield return BuildDialogue(segment.dialogue, segment.appendText);
            }
        }

        IEnumerator WaitForDialogueSegmentSignalToBeTriggered(DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment)
        {
            switch (segment.startSignal)
            {
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.C:
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.A:
                    yield return WaitForUserInput();
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WC:
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WA:
                    yield return new WaitForSeconds(segment.signalDelay);
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.NONE:
                default:
                    break;
            }
        }

        IEnumerator BuildDialogue(string dialogue, bool append = false)
        {
            if (!append)
                // Build the text
                architect.Build(dialogue);
            else
                // Append the text
                architect.Append(dialogue);

            // Wait for the text to finish building
            while (architect.isBuilding)
            {
                if (userPrompt)
                {
                    if (!architect.hurryUp)
                        architect.hurryUp = true;
                    else
                        architect.ForceComplete();
                    userPrompt = false;
                }
                yield return null;
            }
        }

        IEnumerator WaitForUserInput()
        {
            while (!userPrompt)
                yield return null;
            userPrompt = false;
        }
    }
}


