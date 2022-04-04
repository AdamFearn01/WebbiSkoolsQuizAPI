using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebbiSkools_Ltd.Models
{
    public class Quiz
    {
        public int QuizID { get; set; }
        public int UniqueSetID { get; set; }
        public string QuestionText { get; set; }
        public int QuestionID { get; set; }
        public string AnswerOne { get; set; }
        public string AnswerTwo { get; set; }
        public string AnswerThree { get; set; }
        public string AnswerFour { get; set; }
        public string AnswerFive { get; set; }
    }
}
