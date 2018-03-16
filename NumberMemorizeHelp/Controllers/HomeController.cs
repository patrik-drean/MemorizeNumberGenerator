using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using NumberMemorizeHelp.Models;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;



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

        // Method to copy lists 
        public static List<T> CopyList<T>(this List<T> lst)
        {
            List<T> lstCopy = new List<T>();
            foreach (var item in lst)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, item);
                    stream.Position = 0;
                    lstCopy.Add((T)formatter.Deserialize(stream));
                }
            }
            return lstCopy;
        }


    }



    public class HomeController : Controller
    {
        // Declare new List.
        static List<string> Words = new List<string>();
        static List<List<string>> PastPhrases = new List<List<string>>();
        static List<SelectListItem> MinimumWordCount = new List<SelectListItem>();
        static List<SelectListItem> MinimumLettersInWord = new List<SelectListItem>();
        static List<int> UserIDs = new List<int>() { -1 };

        public object ActorArrayList { get; private set; }


        public ActionResult Index()
        {
            // Check for cookie and add it if no cookie exists for that person
            HttpCookie myCookie = Request.Cookies["userid"];

            

            if (myCookie == null || UserIDs.Exists(x => x == Int32.Parse(myCookie.Values["userid"])) == false )
            {
                // Grab ID
                int UserID = 0;
                if (UserIDs.Max() > -1)
                {
                    UserID = UserIDs.Max() + 1;
                    UserIDs.Add(UserID);
                }
                else
                {
                    UserIDs.Add(UserID);
                }

                // Check cookie and set it if no cookie exists
                myCookie = new HttpCookie("userid");

                myCookie.Values.Add("userid", UserID.ToString());

                //set cookie expiry date-time. Made it to last for next 24 hours.
                myCookie.Expires = DateTime.Now.AddHours(6);

                //Most important, write the cookie to client.
                Response.Cookies.Add(myCookie);
            }
           // myCookie.Expires = DateTime.Now.AddDays(-1d);
            //Response.Cookies.Add(myCookie);


            MinimumWordCount.Clear();
            MinimumWordCount.Add(new SelectListItem { Text = "1 letter", Value = "1", Selected = true });
            MinimumWordCount.Add(new SelectListItem { Text = "2 letters", Value = "2" });
            MinimumWordCount.Add(new SelectListItem { Text = "3 letters", Value = "3" });
            MinimumWordCount.Add(new SelectListItem { Text = "4 letters", Value = "4" });
 
            MinimumLettersInWord.Add(new SelectListItem { Text = "1", Value = "1", Selected = true });
            MinimumLettersInWord.Add(new SelectListItem { Text = "2", Value = "2" });
            MinimumLettersInWord.Add(new SelectListItem { Text = "3", Value = "3" });
            MinimumLettersInWord.Add(new SelectListItem { Text = "4", Value = "4" });

            ViewBag.MinimumWordCount = MinimumWordCount;
            ViewBag.MinimumLettersInWord = MinimumLettersInWord;

            // Grab unique ID from cookie
            HttpCookie myCookie1 = Request.Cookies["userid"];

            // Grab all the associated IP address phrases
            List<List<string>> TempFilteredPastPhrases = (from phrase in PastPhrases.CopyList()
                                                          where phrase.ElementAt(0) == myCookie.Values["userid"]
                                                          select phrase).ToList();
            List<List<string>> FilteredPastPhrases = TempFilteredPastPhrases.CopyList();

            //  List<Student> lstStudent = db.Students.Where(s => s.DOB < DateTime.Now).ToList().CopyList();

            // Remove the IP addresses from the list
            for (int iCount = 0; iCount < FilteredPastPhrases.Count; iCount++)
            {
                FilteredPastPhrases.ElementAt(iCount).RemoveAt(0);
            }

            ViewBag.PastPhrases = FilteredPastPhrases;

            return View();
        }

        [HttpPost]
        public ActionResult Index([Bind(Include = "Number")] InputNumber inputNumber, FormCollection form, Boolean RegeneratePhraseAgain = false)
        {
            ViewBag.MinimumWordCount = MinimumWordCount;

            if (ModelState.IsValid)
            {
                /***************** Take input and turn into array of numbers *****************/
                // Make sure input is an int
                var result = int.TryParse(form["Number"].ToString(), out int Num);
                if (result)
                {

                    var Numbers = (form["Number"]).ToString().Select(digit => int.Parse(digit.ToString()));
                    int iMinimumLetterCount = Int32.Parse(form["MinimumWordCount"]);


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

                    // To test words
                    Words.Insert(0, "acid");
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
                                "zz",
                                "ce",
                                "ci"});
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
                                "cc",
                                "ck"});
                                    break;
                                }
                            case 8:
                                {
                                    Letters.Add(new List<String>()
                                {"f",
                                "ff",
                                 "v",
                                "vv",
                                "ph"});
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
                    int LetterMatchCount = 0;
                    int LetterMatchCountTotal = 0;
                    String TwoCharacters = "";
                    int iRetryCounter = 0;

                    // This is to iterate over every number
                    while (iRetryCounter < 5)
                    {
                        foreach (String DictionaryWord in Words)
                        {
                            iCount1 = 0;
                            LetterMatchCount = 0;
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
                                        TwoCharacters == "pp" ||
                                        TwoCharacters == "ck" ||
                                        TwoCharacters == "ci" ||
                                        TwoCharacters == "ce" ||
                                        TwoCharacters == "ph")
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
                                                LetterMatchCount++;
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
                                        if (WordStillMatches && WordContainsLetters && (LetterMatchCount >= iMinimumLetterCount || Letters.Count <= (LetterMatchCount + LetterMatchCountTotal)))
                                        {
                                            iCountPlaceHolder = iCount;
                                            LetterMatchCountTotal += LetterMatchCount;
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
                            iRetryCounter = 5;
                        }

                        iRetryCounter++;
                    }
                    /******** Reset View **************/

                    ViewBag.MinimumWordCount = MinimumWordCount;

                    if (iCount == Letters.Count)
                    {
                        // Add to past phrases
                        CompletedPhrase.Insert(0, form["Number"].ToString());

                        // Grab unique ID from cookie
                        HttpCookie myCookie = Request.Cookies["userid"];

                        // Add unique ID on end to display only the associated past phrases
                        CompletedPhrase.Insert(0, myCookie.Values["userid"]);

                        PastPhrases.Add(CompletedPhrase.CopyList());

                        // Grab all the associated ID phrases
                        List<List<string>> TempFilteredPastPhrases = (from phrase in PastPhrases.CopyList()
                                                                      where phrase.ElementAt(0) == myCookie.Values["userid"]
                                                                      select phrase).ToList();
                        List<List<string>> FilteredPastPhrases = TempFilteredPastPhrases.CopyList();

                        //  List<Student> lstStudent = db.Students.Where(s => s.DOB < DateTime.Now).ToList().CopyList();

                        // Remove the ID from the list
                        for (iCount = 0; iCount < FilteredPastPhrases.Count; iCount++)
                        {
                            FilteredPastPhrases.ElementAt(iCount).RemoveAt(0);
                        }

                        ViewBag.PastPhrases = FilteredPastPhrases;
                        ViewBag.RegeneratedNumber = form["Number"].ToString();
                        ViewBag.MinimumWordCountNumber = form["MinimumWordCount"].ToString();
                        CompletedPhrase.RemoveAt(0);

                        return View(CompletedPhrase);
                    }

                    else
                    {
                        // Grab unique ID from cookie
                        HttpCookie myCookie = Request.Cookies["userid"];

                        // Grab all the associated ID phrases
                        List<List<string>> TempFilteredPastPhrases = (from phrase in PastPhrases.CopyList()
                                                                      where phrase.ElementAt(0) == myCookie.Values["userid"]
                                                                      select phrase).ToList();
                        List<List<string>> FilteredPastPhrases = TempFilteredPastPhrases.CopyList();

                        //  List<Student> lstStudent = db.Students.Where(s => s.DOB < DateTime.Now).ToList().CopyList();

                        // Remove the ID from the list
                        for (iCount = 0; iCount < FilteredPastPhrases.Count; iCount++)
                        {
                            FilteredPastPhrases.ElementAt(iCount).RemoveAt(0);
                        }

                        ViewBag.PastPhrases = FilteredPastPhrases;

                        @ViewBag.Incomplete = "There was no match. Try again.";
                        ViewBag.RegeneratedNumber = form["Number"].ToString();
                        ViewBag.MinimumWordCountNumber = form["MinimumWordCount"].ToString();

                        return View();
                    }
                }
                // Return view if input wasn't an int
                else
                {
                    // Grab unique ID from cookie
                    HttpCookie myCookie = Request.Cookies["userid"];

                    // Grab all the associated ID phrases
                    List<List<string>> TempFilteredPastPhrases = (from phrase in PastPhrases.CopyList()
                                                                  where phrase.ElementAt(0) == myCookie.Values["userid"]
                                                                  select phrase).ToList();
                    List<List<string>> FilteredPastPhrases = TempFilteredPastPhrases.CopyList();

                    //  List<Student> lstStudent = db.Students.Where(s => s.DOB < DateTime.Now).ToList().CopyList();

                    // Remove the ID from the list
                    for (int iCount = 0; iCount < FilteredPastPhrases.Count; iCount++)
                    {
                        FilteredPastPhrases.ElementAt(iCount).RemoveAt(0);
                    }

                    ViewBag.PastPhrases = FilteredPastPhrases;
                    ViewBag.RegeneratedNumber = form["Number"].ToString();
                    ViewBag.MinimumWordCountNumber = form["MinimumWordCount"].ToString();
                    @ViewBag.Incomplete = "Please enter a whole number in order to generate anything";

                    return View();

                }
            }

            else
            {
                // Grab unique ID from cookie
                HttpCookie myCookie = Request.Cookies["userid"];

                // Grab all the associated ID phrases
                List<List<string>> TempFilteredPastPhrases = (from phrase in PastPhrases.CopyList()
                                                              where phrase.ElementAt(0) == myCookie.Values["userid"]
                                                              select phrase).ToList();
                List<List<string>> FilteredPastPhrases = TempFilteredPastPhrases.CopyList();

                //  List<Student> lstStudent = db.Students.Where(s => s.DOB < DateTime.Now).ToList().CopyList();

                // Remove the ID from the list
                for (int iCount = 0; iCount < FilteredPastPhrases.Count; iCount++)
                {
                    FilteredPastPhrases.ElementAt(iCount).RemoveAt(0);
                }

                ViewBag.PastPhrases = FilteredPastPhrases;
                ViewBag.RegeneratedNumber = form["Number"].ToString();
                ViewBag.MinimumWordCountNumber = form["MinimumWordCount"].ToString();

                return View();
            }
        }

        public ActionResult ClearPhases()
        {
            // Grab unique ID from cookie
            HttpCookie myCookie = Request.Cookies["userid"];

            // Grab all the associated ID phrases
            List<List<string>> TempFilteredPastPhrases = (from phrase in PastPhrases
                                                          where phrase.ElementAt(0) != myCookie.Values["userid"]
                                                          select phrase).ToList();

            PastPhrases = TempFilteredPastPhrases.CopyList();



            return RedirectToAction("Index");
        }

        /*********************************** One Word Generation Page **********************************************/

        public ActionResult OneWord()
        {
            return View();
        }

        /*********************************** Key page **********************************************/
        public ActionResult what_is_the_mnemonic_major_system()
        {
            return View("what-is-the-mnemonic-major-system");
        }


    }
}