using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using NumberMemorizeHelp.Models;
using System.Reflection;

/*
* XXX Loop multiple times if words aren't found for long numbers
* XXX Minimum word length
* XXX Add to array of phrases to display past results
* XXX Work on view of displaying past results
* Mobile app?
* Regex
* Validation
* Not displaying similar results when regenerating
* Find more nouns to add to list
* Create Video
* Send to Kevin Horsley
*/

namespace NumberMemorizeHelp.Controllers
{
    public static class MainMethods
    {

        /*************** Method to randmoize a list *************/

        static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }



    public class HomeController : Controller
    {
        // Declare new List.
        static List<string> Words = new List<string>();
        static List<List<string>> PastPhrases = new List<List<string>>();

        static List<SelectListItem> MinimumWordCount = new List<SelectListItem>();



        static List<SelectListItem> MinimumLettersInWord = new List<SelectListItem>();

        public object ActorArrayList { get; private set; }

        public ActionResult Index()
        {
            MinimumWordCount.Clear();
            MinimumWordCount.Add(new SelectListItem { Text = "2 letters long", Value = "2" });
            MinimumWordCount.Add(new SelectListItem { Text = "3 letters long", Value = "3", Selected = true });
            MinimumWordCount.Add(new SelectListItem { Text = "4 letters long", Value = "4" });
            MinimumWordCount.Add(new SelectListItem { Text = "5 letters long", Value = "5" });
            MinimumWordCount.Add(new SelectListItem { Text = "6 letters long", Value = "6" });

            MinimumLettersInWord.Add(new SelectListItem { Text = "1", Value = "1", Selected = true });
            MinimumLettersInWord.Add(new SelectListItem { Text = "2", Value = "2" });
            MinimumLettersInWord.Add(new SelectListItem { Text = "3", Value = "3" });
            MinimumLettersInWord.Add(new SelectListItem { Text = "4", Value = "4" });

            ViewBag.MinimumWordCount = MinimumWordCount;
            ViewBag.MinimumLettersInWord = MinimumLettersInWord;

            ViewBag.PastPhrases = PastPhrases;

            return View();
        }

        [HttpPost]
        public ActionResult Index([Bind(Include = "Number")] InputNumber inputNumber, FormCollection form, Boolean RegeneratePhraseAgain = false)
        {
                if (ModelState.IsValid)
            {
                /***************** Take input and turn into array of numbers *****************/

                var Numbers = (form["Number"]).ToString().Select(digit => int.Parse(digit.ToString()));
                int iMinimumWordCount = Int32.Parse(form["MinimumWordCount"]);


                /**************** Load up words into list *************/
                 const string f = "nounlist.txt";

                //FileInfo file = new FileInfo("nounlist.txt");
                // string filePath = System.IO.Path.GetFullPath("nounlist.txt");
                //filePath = "~\\Content\\nounlist.txt";
                // StreamReader sr = new StreamReader(filePath);


                //var logFile = File.ReadAllLines(LOG_PATH);
                //var logList = new List<string>(logFile);
                var outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                var iconPath = Path.Combine(outPutDirectory, "..\\Content\\nounlist.txt");
                string path = new Uri(iconPath).LocalPath;


                using (StreamReader r = new StreamReader(path))
                 {
                     // Use while != null pattern for loop
                     string line;
                     while ((line = r.ReadLine()) != null)
                     {
                         Words.Add(line);
                     }
                 }
                 
    


                //Randomize list
                Words.Shuffle();



                /************* Take input and compare it to list of words ***************/

                List<String> WildCards = new List<String>()
                                {"a",
                                 "e",
                                 "i",
                                 "o",
                                 "u",
                                 "h",
                                 "w",
                                 "y",
                                 "-"};

                List<String> CompletedPhrase = new List<String>();
                List<List<String>> Letters = new List<List<String>>();

                String TestSequence = "";
                String DictionarySequence = "";
                int iCount = 0;
                int iCount1 = 0;
                int iCount2 = 0;
                int iCountPlaceHolder = 0;


                // Change the inputted number into each list within the master list
                foreach (int Number in Numbers)
                {
                    switch (Number)
                    {
                        case 0:
                            {
                                Letters.Add(new List<String>()
                                {"s",
                                "ss",
                                "z",
                                "zz"});
                                break;
                            }
                        case 1:
                            {
                                Letters.Add(new List<String>()
                                {"d",
                                "dd",
                                "t",
                                "tt"});
                                break;
                            }
                        case 2:
                            {
                                Letters.Add(new List<String>()
                                {"n",
                                 "nn"});
                                break;
                            }
                        case 3:
                            {
                                Letters.Add(new List<String>()
                                {"m",
                                "mm"});
                                break;
                            }
                        case 4:
                            {
                                Letters.Add(new List<String>()
                                {"r",
                                "rr"});
                                break;
                            }
                        case 5:
                            {
                                Letters.Add(new List<String>()
                                {"l",
                                "ll"});
                                break;
                            }
                        case 6:
                            {
                                Letters.Add(new List<String>()
                                {"j",
                                "jj",
                                 "sh",
                                 "ch",
                                "g",
                                "gg"});
                                break;
                            }
                        case 7:
                            {
                                Letters.Add(new List<String>()
                                {"k",
                                "kk",
                                 "c",
                                "cc"});
                                break;
                            }
                        case 8:
                            {
                                Letters.Add(new List<String>()
                                {"f",
                                "ff",
                                 "v",
                                "vv"});
                                break;
                            }
                        case 9:
                            {
                                Letters.Add(new List<String>()
                                {"b",
                                "bb",
                                 "p",
                                "pp"});
                                break;
                            }
                    }
                }

                Boolean WordStillMatches = true;
                Boolean WordContainsLetters = false;
                String TwoCharacters = "";
                int iRetryCounter = 0;

                // This is to iterate over every number
                while (iRetryCounter < 3)
                {
                    foreach (String DictionaryWord in Words)
                    {
                        iCount1 = 0;

                        // Determine if the prior word was successfully added or not
                        iCount = iCountPlaceHolder;

                        DictionarySequence = "";
                        TestSequence = "";
                        WordContainsLetters = false;


                        // Don't loop if the phrase has already been completed
                        if (iCount < Letters.Count)
                        {
                            WordStillMatches = true;
                            while (WordStillMatches)
                            {
                                WordStillMatches = false;
                                TwoCharacters = "";

                                if (DictionarySequence.Length < (DictionaryWord.Length - 1))
                                {
                                    TwoCharacters += DictionaryWord[iCount1];
                                    TwoCharacters += DictionaryWord[iCount1 + 1];
                                }

                                // Load up the dictionary word to however far it's been completed 
                                if (TwoCharacters == "ss" ||
                                    TwoCharacters == "zz" ||
                                    TwoCharacters == "dd" ||
                                    TwoCharacters == "tt" ||
                                    TwoCharacters == "nn" ||
                                    TwoCharacters == "mm" ||
                                    TwoCharacters == "rr" ||
                                    TwoCharacters == "jj" ||
                                    TwoCharacters == "sh" ||
                                    TwoCharacters == "ch" ||
                                    TwoCharacters == "gg" ||
                                    TwoCharacters == "kk" ||
                                    TwoCharacters == "cc" ||
                                    TwoCharacters == "ff" ||
                                    TwoCharacters == "vv" ||
                                    TwoCharacters == "bb" ||
                                    TwoCharacters == "ll" ||
                                    TwoCharacters == "pp")
                                {
                                    DictionarySequence += TwoCharacters;
                                    iCount1++;
                                }
                                else
                                {
                                    DictionarySequence += DictionaryWord[iCount1];
                                }

                                if (iCount < Letters.Count)
                                {
                                    // This it to iterate over every possible letter for each number
                                    for (iCount2 = 0; iCount2 < Letters[iCount].Count; iCount2++)
                                    {
                                        if (DictionarySequence == TestSequence + Letters[iCount][iCount2])
                                        {
                                            TestSequence = TestSequence + Letters[iCount][iCount2];
                                            iCount++;
                                            WordStillMatches = true;
                                            WordContainsLetters = true;
                                            break;
                                        }

                                        if (iCount == Letters.Count)
                                        {
                                            break;
                                        }
                                    }
                                }

                                // This it to iterate over the wildcards
                                if (WordStillMatches == false)
                                {
                                    foreach (String WildCard in WildCards)
                                    {
                                        if (DictionarySequence == TestSequence + WildCard)
                                        {
                                            WordStillMatches = true;
                                            TestSequence = TestSequence + WildCard;
                                            break;
                                        }
                                    }
                                }

                                // Stop looping if the wordsequence matches the dictionary word length
                                if (DictionarySequence.Length == DictionaryWord.Length)
                                {
                                    // Determine if the word officially matches
                                    if (WordStillMatches && WordContainsLetters && DictionaryWord.Length >= iMinimumWordCount)
                                    {
                                        iCountPlaceHolder = iCount;

                                        // Add the word to the official list
                                        CompletedPhrase.Add((DictionaryWord + " "));
                                    }
                                    WordStillMatches = false;
                                }

                                iCount1++;
                            }
                        }
                    }
                    if (iCount == Letters.Count)
                    {
                        iRetryCounter = 3;
                    }

                    iRetryCounter++;
                }
                /******** Reset View **************/

                ViewBag.MinimumWordCount = MinimumWordCount;

                if (iCount == Letters.Count)
                {
                    // Add to past phrases
                    CompletedPhrase.Insert(0, form["Number"].ToString());
                    PastPhrases.Add(CompletedPhrase);
                    ViewBag.PastPhrases = PastPhrases;
                    ViewBag.RegeneratedNumber = form["Number"].ToString();
                    ViewBag.MinimumWordCountNumber = form["MinimumWordCount"].ToString();

                    return View(CompletedPhrase);
                }

                else
                {
                    ViewBag.PastPhrases = PastPhrases;
                    @ViewBag.Incomplete = "There was no match. Try again.";
                    ViewBag.RegeneratedNumber = form["Number"].ToString();
                    ViewBag.MinimumWordCountNumber = form["MinimumWordCount"].ToString();

                    return View();
                }
            }

            ViewBag.MinimumWordCount = MinimumWordCount;
            ViewBag.PastPhrases = PastPhrases;

            return View();
        }

        public ActionResult ClearPhases()
        {
            PastPhrases.Clear();

            return RedirectToAction("Index");
        }

    }
}