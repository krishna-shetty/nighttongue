using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using Yarn.Markup;
using Yarn.Unity;

namespace Audio
{
    public class AudioLetterTypewriter : MonoBehaviour, IAsyncTypewriter
    {
        [Header("References")]
        public TMP_Text text;
        public TMP_Text characterNameText; // drag from LinePresenter
        public GameObject audioObject;

        [Header("Settings")]
        public float charactersPerSecond = 60f;
        public float minTimeBetweenSounds = 0.1f;
        [SerializeField] private int wordsPerSound = 2;
        [SerializeField] private float delayAfterVowel = 0.4f;

        public List<IActionMarkupHandler> ActionMarkupHandlers { get; set; } = new();

        private float lastSoundTime;
        private bool previousWasWordChar;
        private bool previousWasVowel;
        private int wordCounter = 0;
        

        public async YarnTask RunTypewriter(MarkupParseResult line, CancellationToken cancellationToken)
        {
            if (text == null)
            {
                Debug.LogWarning("Missing TMP_Text");
                return;
            }

            text.maxVisibleCharacters = 0;
            text.text = line.Text;

            foreach (var handler in ActionMarkupHandlers)
            {
                handler.OnLineDisplayBegin(line, text);
            }

            double secondsPerChar = charactersPerSecond > 0 ? 1.0 / charactersPerSecond : 0;

            var textInfo = text.GetTextInfo(line.Text);
            int visibleCount = textInfo.characterCount;

            double accumulatedDelay = secondsPerChar;

            for (int i = 0; i < visibleCount; i++)
            {
                while (!cancellationToken.IsCancellationRequested &&
                       accumulatedDelay < secondsPerChar)
                {
                    var t0 = Time.timeAsDouble;
                    await YarnTask.Yield();
                    var t1 = Time.timeAsDouble;
                    accumulatedDelay += t1 - t0;
                }

                foreach (var handler in ActionMarkupHandlers)
                {
                    await handler
                        .OnCharacterWillAppear(i, line, cancellationToken)
                        .SuppressCancellationThrow();
                }

                text.maxVisibleCharacters++;

                // Character extraction
                if (i < text.textInfo.characterCount)
                {
                    var charInfo = text.textInfo.characterInfo[i];
                    
                    //Debug.Log($"Character {charInfo.character} is visible");
                    HandleCharacter(charInfo.character);

                    //if (charInfo.isVisible)
                    //{
                    //    Debug.Log($"Character {charInfo.character} is visible");
                    //    HandleCharacter(charInfo.character);
                    //}
                }//

                accumulatedDelay -= secondsPerChar;
            }

            text.maxVisibleCharacters = visibleCount;

            foreach (var handler in ActionMarkupHandlers)
            {
                handler.OnLineDisplayComplete();
            }
        }

        private void HandleCharacter(char c)
        {
            if (audioObject == null || characterNameText == null)
                return;
            
            CheckWordBoundary(c);
            if (wordCounter % wordsPerSound != 0)
                return;

            if (WasVowel(c))
            {
                minTimeBetweenSounds += 0.5f;
                return;
            }
            minTimeBetweenSounds -= 0.5f;

            if (Char.IsWhiteSpace(c))
                return;

            if (Time.time - lastSoundTime < minTimeBetweenSounds)
                return;

            string speaker = characterNameText.text.ToLower();
            string suffix = GetSuffix(c);

            if (suffix != null)
            {
                string eventName = $"Play_dx_{speaker}_{suffix}";
                //Debug.Log($"Playing {eventName}");
                AkUnitySoundEngine.PostEvent(eventName, audioObject);
            }

            lastSoundTime = Time.time;
        }

        private string GetSuffix(char c)
        {
            if (Char.IsLetter(c)) return Char.ToString(c).ToLower();
            //if (char.IsDigit(c)) return "number";

            //switch (c)
            //{
            //    case '.':
            //    case '!':
            //    case '?': return "end";
            //    case ',': return "pause";
            //    default: return "symbol";
            //}
            return null;
        }

        private void CheckWordBoundary(char c)
        {
            bool isWordChar = Char.IsLetterOrDigit(c);
            if (!isWordChar && previousWasWordChar)
            {
                wordCounter++;
                previousWasVowel = false;
            }
            previousWasWordChar = isWordChar;
        }

        private bool WasVowel(char c)
        {
            bool isVowel = Char.IsLetter(c) && "aeiou".Contains(Char.ToLower(c));
            if (previousWasVowel)
            {
                previousWasVowel = false;
                return true;
            }
            previousWasVowel = isVowel;
            return false;
        }
        
        public void PrepareForContent(MarkupParseResult line)
        {
            if (text == null) return;

            text.maxVisibleCharacters = 0;
            text.text = line.Text;

            foreach (var handler in ActionMarkupHandlers)
            {
                handler.OnPrepareForLine(line, text);
            }
        }

        public void ContentWillDismiss()
        {
            foreach (var handler in ActionMarkupHandlers)
            {
                handler.OnLineWillDismiss();
            }
        }
    }
}