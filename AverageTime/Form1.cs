using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CSALMongo;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace AverageTime
{
    public partial class Form1 : Form
    {

        //should change the name of the database
        public const string DB_URL = "mongodb://localhost:27017/csaldata";
        //protected MongoDatabase testDB = null;
        List<String> studentsInClass = new List<string>();

        String Tags = "Lesson No." + "\t" + "Mean Time" + "\t" + "Mean time to complete" + "\n";

        public Form1()
        {
            InitializeComponent();
            // Start 
            try
            {
                //query from the database
                var db = new CSALDatabase(DB_URL);
                var students = db.FindStudents();
                var lessons = db.FindLessons();
                var classes = db.FindClasses();



                // find the target classes
                foreach (var oneClass in classes)
                {
                    // ace | kingwilliam | main | tlp
                    // if (oneClass.ClassID == "aec" || oneClass.ClassID == "kingwilliam" || oneClass.ClassID == "main" || oneClass.ClassID == "tlp")
                    if (oneClass.ClassID == "pilot1_lai1" || oneClass.ClassID == "pilot1_lai2" || oneClass.ClassID == "pilot1_marietta1" ||
                        oneClass.ClassID == "pilot1_marietta2")
                    {

                        foreach (String student in oneClass.Students)
                        {
                            if (!String.IsNullOrWhiteSpace(student) && !String.IsNullOrWhiteSpace(oneClass.ClassID))
                            {
                                String cS = oneClass.ClassID + "-" + student;
                                studentsInClass.Add(cS);
                            }
                                   }
                    }

                    // lai | marietta
                    if (oneClass.ClassID == "pilot1_aecn" || oneClass.ClassID == "pilot1_ptp1" || oneClass.ClassID == "pilot1_ptp2" || oneClass.ClassID == "pilot1_tlp")
                    {
                        foreach (String student in oneClass.Students)
                        { 
                            if (!String.IsNullOrWhiteSpace(student) && !String.IsNullOrWhiteSpace(oneClass.ClassID))
                            {
                                String cS = oneClass.ClassID + "-" + student;
                                studentsInClass.Add(cS);
                            }
                        }
                    }
                }

         
                //this.richTextBox1.Text = needStudentsA.Count.ToString();

                List<double> totalTimeForLesson = new List<double>();
                List<double> totalNumberStudents = new List<double>();
                List<double> completeTime = new List<double>();
                List<double> totalCompleteNumberStudents = new List<double>();

                // initial the list
                totalNumberStudents = initialList(totalNumberStudents);
                totalTimeForLesson = initialList(totalTimeForLesson);
                completeTime = initialList(completeTime);
                totalCompleteNumberStudents = initialList(totalCompleteNumberStudents);

                foreach (String student in studentsInClass)
                {
                    int lessonNumber = 0;
                    if (!String.IsNullOrWhiteSpace(student))
                    {
                        // need to be careful here, may not right
                        List<List<double>> allLessonForOne = getInfoForOne(student);
                        

                        if (allLessonForOne == null || allLessonForOne.Count < 1)
                        {
                            continue;
                        }
                        else
                        {
                            foreach (List<double> oneLesson in allLessonForOne)
                            {
                                double totalTime = 0;
                                int numberTrial = 0;
                                if (oneLesson != null)
                                {
                                    foreach (double time in oneLesson)
                                    {
                                        totalTime += time;
                                        numberTrial += 1;
                                    }

                                    //
                                    totalNumberStudents[lessonNumber] += numberTrial;
                                    totalTimeForLesson[lessonNumber] += totalTime;
                                }
                               

                                lessonNumber++;
                            }
                        }

                       
                        //allText += recordCount.ToString() + "\t" + getInfoForOne(studentRecord) + "\n";
                    }
                }

                foreach (String student in studentsInClass)
                {
                    int lessonNumber = 0;
                    if (!String.IsNullOrWhiteSpace(student))
                    {
                        List<List<double>> allCompleteLessonForOne = getCompleteInfoForOne(student);
                        if (allCompleteLessonForOne == null || allCompleteLessonForOne.Count < 1)
                        {
                            continue;
                        }
                        else
                        {
                            foreach (List<double> oneLesson in allCompleteLessonForOne)
                            {
                                double totalTime = 0;
                                int numberTrial = 0;
                                if (oneLesson != null)
                                {
                                    foreach (double time in oneLesson)
                                    {
                                        totalTime += time;
                                        numberTrial += 1;
                                    }

                                    //
                                    totalCompleteNumberStudents[lessonNumber] += numberTrial;
                                    completeTime[lessonNumber] += totalTime;
                                }

                                lessonNumber++;
                            }
                        }
                    }
                }


                int recordID = 0;
                this.richTextBox1.Text = Tags;


                string[] lines = System.IO.File.ReadAllLines(@"D:\CSAL\Tools\AverageTime\Lessons.txt");
                List<string> lessonNames = new List<string>();
                List<int> lessonOrder = new List<int>();
                foreach (string line in lines)
                {
                    string lesson = line.Split(new char[] { ':' })[0];
                    lessonNames.Add(lesson);
                }

                foreach (string lesson in lessonNames)
                {
                    string resultString = Regex.Match(lesson, @"\d+").Value;
                    int lessonID = Int32.Parse(resultString);
                    lessonOrder.Add(lessonID);
                }


                foreach (int i in lessonOrder)
                {
                    this.richTextBox1.AppendText("lesson" + (i).ToString() + "\t" + (totalTimeForLesson[i-1] / totalNumberStudents[i-1]).ToString() + "\t" + (completeTime[i-1] / totalCompleteNumberStudents[i-1]).ToString() + "\n");
                }

            }
            catch (Exception e)
            {
                e.GetBaseException();
                e.GetType();

            }
        }

        // initial The list
        public List<double> initialList(List<double> list1)
        {
            for (int i = 0; i < 100; i++)
            {
                list1.Add(0.0);
            }
            return list1;
        }

        public List<List<double>> getInfoForOne(String studentRecord)
        {
            List<List<double>> allRecord = new List<List<double>>();

            for (int i = 1; i < 35; i++)
            {
                var lessonId = "lesson" + i.ToString();
                String classID = studentRecord.Split(new Char[] { '-' })[0];
                String subjectID = studentRecord.Split(new Char[] { '-' })[1];
                List<double> allRecords = getPerLesson(subjectID, lessonId);

                // student didn't attend the lesson
                if (allRecords == null || allRecords.Count < 1)
                {
                    //allRecords.Add(77777);
                    allRecord.Add(allRecords);
                }
                else
                {
                    allRecord.Add(allRecords);
                }
            }
            return allRecord;
        }
        
        public List<double> getPerLesson(String studentName, String lessonID)
        {
            var db = new CSALDatabase(DB_URL);
            int lastTurnID = 99, attempCount = 0;
            var oneTurn = db.FindTurns(lessonID, studentName);
            List<double> attempTime = new List<double>();

            if (oneTurn == null || oneTurn.Count < 1 || oneTurn[0].Turns.Count < 1)
            {
                return null;
            }
            else
            {
                // calculate total time of every Attempt
                // list starts at 0
                foreach (var turn in oneTurn[0].Turns)
                {
                    if (turn.TurnID < lastTurnID)
                    {
                        attempCount++;
                        double turnDura = (int)turn.Duration;
                        turnDura = turnDura / 1000;
                        attempTime.Add(turnDura);
                    }
                    else
                    {
                        double turnDura = (int)turn.Duration;
                        attempTime[attempCount-1] += turnDura / 1000;
                    }
                    lastTurnID = turn.TurnID;
                }
            }

            return attempTime;
        }

        // complete time for one students
        public List<List<double>> getCompleteInfoForOne(String studentRecord)
        {
            List<List<double>> allRecord = new List<List<double>>();

            for (int i = 1; i < 35; i++)
            {
                var lessonId = "lesson" + i.ToString();
                String classID = studentRecord.Split(new Char[] { '-' })[0];
                String subjectID = studentRecord.Split(new Char[] { '-' })[1];
                List<double> allRecords = getPerLessonComplete(subjectID, lessonId);

                // student didn't attend the lesson
                if (allRecords == null || allRecords.Count < 1)
                {
                    //allRecords.Add(77777);
                    allRecord.Add(allRecords);
                }
                else
                {
                    allRecord.Add(allRecords);
                }
            }
            return allRecord;
        }

        // complete time per lesson
        public List<double> getPerLessonComplete(String studentName, String lessonID)
        {
            var db = new CSALDatabase(DB_URL);
            int lastTurnID = 99, attempCount = 0;
            var oneTurn = db.FindTurns(lessonID, studentName);
            List<double> attempTime = new List<double>();
            List<double> completeAttemp = new List<double>();

            if (oneTurn == null || oneTurn.Count < 1 || oneTurn[0].Turns.Count < 1)
            {
                return null;
            }
            else
            {
                // calculate total time of every Attempt
                // list starts at 0
                foreach (var turn in oneTurn[0].Turns)
                {
                    if (turn.TurnID < lastTurnID)
                    {
                        attempCount++;
                        double turnDura = (int)turn.Duration;
                        turnDura = turnDura / 1000;
                        attempTime.Add(turnDura);
                    }
                    else
                    {
                        double turnDura = (int)turn.Duration;
                        attempTime[attempCount - 1] += turnDura / 1000;
                    }

                    lastTurnID = turn.TurnID;

                    foreach (var transition in turn.Transitions)
                    {
                        if (transition.RuleID == "End")
                        {
                            completeAttemp.Add(attempTime[attempCount - 1]);
                        }
                    }

                    if (turn.Input.Event == "End")
                    {
                        completeAttemp.Add(attempTime[attempCount - 1]);
                    }
                }
            }
            return completeAttemp;
        }
    }
}
