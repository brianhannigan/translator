using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TranslatorBackend.Interfaces;

namespace Translator.Processors
{
    internal class TesseractProcessor
    {
        public static List<List<IWordInfo>> GroupWordsByLocation(List<IWordInfo> words, int verticalThreshold = 20, int horizontalThreshold = 20)
        {
            // Sort words vertically by their top position.
            words.Sort((w1, w2) => w1.BoundingBox.Y1.CompareTo(w2.BoundingBox.Y1));

            var groupedWords = new List<List<IWordInfo>>();

            foreach (var word in words)
            {
                bool addedToGroup = false;

                // Check against each existing group AND consider both vertical and horizontal proximity
                for (int i = 0; i < groupedWords.Count; i++)
                {
                    var group = groupedWords[i];

                    // Check if the word is vertically aligned with any word in the group
                    if (group.Any(w =>
                        Math.Abs(word.BoundingBox.Y1 - w.BoundingBox.Y1) <= verticalThreshold ||
                        Math.Abs(word.BoundingBox.Y2 - w.BoundingBox.Y2) <= verticalThreshold))
                    {
                        // Check horizontal distance to the closest word in the group
                        var closestWord = group.OrderBy(w => Math.Abs(word.BoundingBox.X1 - w.BoundingBox.X1)).First();
                        var horizontalDistance = word.BoundingBox.X1 - (closestWord.BoundingBox.X1 + closestWord.BoundingBox.Width);

                        if (horizontalDistance <= horizontalThreshold)
                        {
                            group.Add(word);
                            addedToGroup = true;

                            // Re-sort the group horizontally after adding the new word
                            group.Sort((w1, w2) => w1.BoundingBox.X1.CompareTo(w2.BoundingBox.X1));
                            break;
                        }
                    }
                }

                // If the word doesn't belong to any existing group, create a new group.
                if (!addedToGroup)
                {
                    groupedWords.Add(new List<IWordInfo> { word });
                }
            }

            return groupedWords;
        }

        public static Rect GetGroupRectangle(List<IWordInfo> wordGroup)
        {
            if (wordGroup == null || wordGroup.Count == 0)
            {
                return Rect.Empty;
            }

            int left = wordGroup.Min(w => w.BoundingBox.X1);
            int top = wordGroup.Min(w => w.BoundingBox.Y1);
            int right = wordGroup.Max(w => w.BoundingBox.X2);
            int bottom = wordGroup.Max(w => w.BoundingBox.Y2);

            return new Rect(left, top, right - left, bottom - top);
        }
    }
}
