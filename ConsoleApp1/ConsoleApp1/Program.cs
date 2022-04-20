using System;

namespace ConsoleApp1
{
    using System;

    public class Person
    {
        public static int MembriDellaFamiglia = 0;
        public static void  PrintIncoraggiamento()
        {
            Console.WriteLine("bELLA FAMIGLIA");
        }
        public string Name { get; set; }
        public string FamilyName { get; set; }
        public DateTime DOB { get; set; }

        public Person(string namePersonaNuova, string familyNamePersonaNuova, DateTime DOBPersonaNuova)
        { 
           this.Name = namePersonaNuova;    
           this.FamilyName = familyNamePersonaNuova;
           this.DOB   = DOBPersonaNuova;
           MembriDellaFamiglia++;
        }
        public void PrintData()
        {
            Console.WriteLine($"Il nome di questa persone è: {this.Name}, " +
                $"il cognome è: {this.FamilyName}, la data di nascita" +
                $" è: {this.DOB.ToString("dd/MM/yyyy")}");
        }
    }

    public class Program
    {
        public static void Main()
        {
            Person zio=new Person("zio ciccio", "Moschetti", DateTime.Now.AddMilliseconds(-1));

    
            Person cuginoMarco= new Person("Marco", "moschetti", DateTime.Now.AddYears(-1000));
    
            zio.PrintData();
            cuginoMarco.PrintData();
            Person.PrintIncoraggiamento();
            Console.WriteLine($"ho creato {Person.MembriDellaFamiglia} membri");
            Console.ReadLine();
           

           
                
                
                }


    }

}
