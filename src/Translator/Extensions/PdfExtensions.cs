using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Translator.Interfaces;
using Translator.PDF;

namespace Translator.Extensions
{
    internal static class PdfExtensions
    {
        public static IEnumerable<IPageContent> GetPageContents(this UglyToad.PdfPig.Content.Page page, bool includeImages = true, double gap = 0.3)
        {
            if (page == null)
                yield break;

            var contentItems = new List<ILocationPageContent>();
            foreach (var letter in page.Letters)
            {
                contentItems.Add(new LetterPageContent(letter));
            }

            if (includeImages)
            {
                int index = 0;
                foreach (var image in page.GetImages())
                {
                    contentItems.Add(new ImagePageContent(image, index++));
                }
            }

            var orderedAndGrouped = contentItems
               .OrderByDescending(x => Math.Round(x.Top))
               .ThenBy(x => x.Left)
               .GroupBy(x => Math.Round(x.Top))
               .ToList();

            StringBuilder currentTextBlock = new StringBuilder();
            foreach (var group in orderedAndGrouped)
            {
                // Try to cast the entire group to a collection of LetterPageContent
                var lettersInGroup = group.OfType<LetterPageContent>().ToList();
                if (int.Equals(lettersInGroup.Count, group.Count())) // Check if all items were successfully cast to LetterPageContent
                {
                    string processedText = ProcessRowToWordsFromLetters(lettersInGroup);
                    currentTextBlock.AppendLine(processedText);
                }
                else
                {
                    if (currentTextBlock.Length > 0)
                    {
                        yield return new StringPageContent(currentTextBlock.ToString());
                        currentTextBlock.Clear();
                    }

                    var imagesInGroup = group.OfType<ImagePageContent>().ToList();
                    foreach (ImagePageContent image in imagesInGroup)
                    {
                        yield return image;
                    }
                }
            }

            if (currentTextBlock.Length > 0)
            {
                yield return new StringPageContent(currentTextBlock.ToString());
            }
        }

        private static string ProcessRowToWordsFromLetters(IEnumerable<ILetterPageContent> groupedLetters, double gap = 0.3)
        {
            if (groupedLetters == null)
                return string.Empty;

            StringBuilder rowBuilder = new StringBuilder();
            double previousRight = 0;

            foreach (ILetterPageContent letter in groupedLetters)
            {
                double chunkDif = letter.Content.StartBaseLine.X - previousRight;
                int padding = (int)Math.Max(0, (letter.Content.StartBaseLine.X * gap) - rowBuilder.Length); // Calculate padding dynamically
                string paddedText = letter.Content.Value.PadLeft(padding, ' ');

                if (chunkDif > gap)
                {
                    rowBuilder.Append(paddedText);
                }
                else
                {
                    rowBuilder.Append(letter.Content.Value);
                }

                previousRight = letter.Content.EndBaseLine.X;
            }

            return rowBuilder.ToString().Trim().Replace("\r\n", "");
        }
    }
}
