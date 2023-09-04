using System.Text.RegularExpressions;

namespace Wox.Plugin.Runner
{
  public class Command
    {
        public string Shortcut { get; set; } = "";
        public string Description { get; set; } = "";
        public string Path { get; set; } = "";
        public string WorkingDirectory { get; set; } = "";
        public string ArgumentsFormat { get; set; } = "";

        public int TermsCount
        {
            get
            {
                if (ArgumentsFormat == null) return 0;

                // this is the "unlimited" terms symbol
                if (ArgumentsFormat.Contains("{*}"))
                {
                  return 50;
                }

                // return the number of {} terms
                return Regex.Matches(ArgumentsFormat, "{.*?}").Count;
            }
        }
    }
}
