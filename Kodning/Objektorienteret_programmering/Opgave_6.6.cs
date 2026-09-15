public class Class1
{
    public void ConvertCelsiusToFahrenheit()
    {
        for(double c=-5; c<=40;c+=0.5)
        {
            double f = 32+(9.0/5.0*c);
            Console.WriteLine("C: " + c + " F: " + f);
            

        }
    }
}