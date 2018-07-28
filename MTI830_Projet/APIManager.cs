using MTI830_Projet.DTO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MTI830_Projet
{
    public static class APIManager
    {
        public const string LANG = "en";
        public const string NOT_FOUND = "404 Not Found";

        public async static Task<EntryDTO> GetWordEntry(string word, string lang = LANG)
        {
            using (var response = await CustomHttpClient.Client()
                .GetAsync(@"entries/" + lang + "/" + word + "/regions=US; definitions")
                .ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var content = response.Content)
                    {
                        string rawcontent = await content.ReadAsStringAsync();

                        JObject json = JObject.Parse(rawcontent);
                        EntryDTO entry = JsonConvert.DeserializeObject<EntryDTO>(rawcontent);
                        entry.Word = word;
                        return entry;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public async static Task<EntryDTO> GetLemmaEntry(string word, string lang = LANG)
        {
            using (var response = await CustomHttpClient.Client()
                .GetAsync(@"inflections/" + lang + "/" + word)
                .ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (var content = response.Content)
                    {
                        string rawcontent = await content.ReadAsStringAsync();

                        JObject json = JObject.Parse(rawcontent);
                        EntryDTO entry = JsonConvert.DeserializeObject<EntryDTO>(rawcontent);
                        entry.Word = word;
                        return entry;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public static List<LexicalEntry> GetLexicalEntries(this EntryDTO entry)
        {
            return entry.Results.SelectMany(r => r.LexicalEntries).ToList();
        }

        public static List<string> GetFirstEntryDefinitions(this EntryDTO entry)
        {
            List<Result> results = entry.Results;
            if (results.Any())
            {
                List<LexicalEntry> lexicalEntries = results.First().LexicalEntries;
                if (lexicalEntries.Any())
                {
                    List<Entry> entries = lexicalEntries.First().Entries;
                    if (entries.Any())
                    {
                        List<Sense> senses = entries.First().Senses;
                        if (senses.Any())
                        {
                            List<string> definitions = senses.First().Definitions;
                            return definitions;
                        }
                    }
                }
            }
            
            return new List<string>();
        }

        public static List<string> GetNounDefinitions(this EntryDTO entry)
        {
           return entry.Results
                    .SelectMany(r => r.LexicalEntries).Where(lex => lex.LexicalCategory == "Noun")
                    .Where(lex => lex.Entries != null)
                    .SelectMany(lex => lex.Entries)
                    .Where(e => e.Senses != null)
                    .SelectMany(e => e.Senses)
                    .Where(s => s.Definitions != null)
                    .SelectMany(s => s.Definitions)
                    .ToList();
        }

        public static async Task<IEnumerable<EntryDTO>> GetLemmatizedNouns(this string sentence, HashSet<string> exceptions)
        {
            List<string> words = Regex.Matches(sentence, @"[\wé-]+(?:'[\wé]+)?").Cast<Match>().Select(m => m.Value).Distinct().ToList();
            List<string> semanticNouns = Parser.Main.ExtractNounsFromSemantics(sentence);

            List<EntryDTO> nouns = new List<EntryDTO>();
            HashSet<string> done = new HashSet<string>();
            foreach (string word in words)
            {
                EntryDTO entry = await GetWordEntry(word);
                if (entry == null) entry = await GetLemmaEntry(word);
                if (entry == null) continue;

                entry = await entry.GetNounEntry(semanticNouns);
                if (entry != null && !exceptions.Contains(entry.Word) && !done.Contains(entry.Word))
                {
                    done.Add(entry.Word);
                    nouns.Add(entry);
                }
            }

            return nouns;
        }

        /**
         *  Returns the associated lemmatized noun EntryDTO of the initial entry, or Null if it has no such association. 
         */
        private static async Task<EntryDTO> GetNounEntry(this EntryDTO entry, List<string> probableNouns)
        {
            if (entry == null) return null;

            var entryLex = entry.GetLexicalEntries();
            if (entryLex == null) return null;

            var entryLexNouns = entryLex.Where(lex => lex.LexicalCategory == "Noun");

            /*
             *  If either of the following conditions are met, it is probably noun:
             *      - There are lexical entries with the Noun category AND the semantical analysis tagged it as a noun.
             *      - The only lexical entries of this word are in the Noun category.
             */
            if ((entryLexNouns.Any() && probableNouns.Contains(entry.Word)) || entryLex.Count() == entryLexNouns.Count())
            {
                // If it has inflections, it's a lemma
                var inflections = entryLexNouns.Where(lex => lex.InflectionOf != null).SelectMany(lex => lex.InflectionOf).ToList();
                if (inflections.Any())
                    // Return the full EntryDTO for the lemma
                    return await GetWordEntry(inflections.First().Id);
                else
                    // Return the initial entry, since it's probably a root noun already
                    return entry;
            }
            return null;
        }
    }
}
