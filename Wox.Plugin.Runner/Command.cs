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

        public bool UnlimitedTerms
        {
          get { return ArgumentsFormat != null && ArgumentsFormat.Contains("{*}"); }
        }

        public int TermsCount
        {
            get
            {
                if (ArgumentsFormat == null) return 0;

                // arbitrarily high count for correctish ordering
                if (UnlimitedTerms)
                {
                  return 20;
                }

                // return the number of {} terms
                return Regex.Matches(ArgumentsFormat, "{.*?}").Count;
            }
        }
    }
}
