namespace Gideon
{
    class DictionaryData
    {
        public class DictionaryResults
        {
            public Metadata metadata { get; set; }
            public Result[] results { get; set; }
        }

        public class Metadata
        {
            public string provider { get; set; }
        }

        public class Result
        {
            public string id { get; set; }
            public string language { get; set; }
            public Lexicalentry[] lexicalEntries { get; set; }
            public string type { get; set; }
            public string word { get; set; }
        }

        public class Lexicalentry
        {
            public Entry[] entries { get; set; }
            public string language { get; set; }
            public string lexicalCategory { get; set; }
            public Pronunciation[] pronunciations { get; set; }
            public string text { get; set; }
        }

        public class Entry
        {
            public string[] etymologies { get; set; }
            public Grammaticalfeature[] grammaticalFeatures { get; set; }
            public string homographNumber { get; set; }
            public Sense[] senses { get; set; }
        }

        public class Grammaticalfeature
        {
            public string text { get; set; }
            public string type { get; set; }
        }

        public class Sense
        {
            public string[] definitions { get; set; }
            public string[] domains { get; set; }
            public string id { get; set; }
            public Subsense[] subsenses { get; set; }
            public Example1[] examples { get; set; }
            public string[] registers { get; set; }
        }

        public class Subsense
        {
            public string[] definitions { get; set; }
            public string[] domains { get; set; }
            public string id { get; set; }
            public Example[] examples { get; set; }
        }

        public class Example
        {
            public string text { get; set; }
        }

        public class Example1
        {
            public string text { get; set; }
        }

        public class Pronunciation
        {
            public string audioFile { get; set; }
            public string[] dialects { get; set; }
            public string phoneticNotation { get; set; }
            public string phoneticSpelling { get; set; }
        }

    }
}
