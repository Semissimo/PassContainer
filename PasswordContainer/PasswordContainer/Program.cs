using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Collections;
using System.Security.Policy;

namespace PasswordContainer
{
    internal class Program
    {

        static SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\ACER\Desktop\C#\PasswordContainer\PasswordContainer\Password.mdf;Integrated Security=True");

        static void Main(string[] args)
        {
            string username;
            string selezionesito;
            string newpass;
            int id = 0;

            List<string> sito = new List<string>();
            List<string> pass = new List<string>(); //le due liste lavorano in parallelo
            
            Console.WriteLine("Benvenuto nel contenitore di passwords");
            int tentativi = 0;

            while (tentativi < 3) {
                Console.WriteLine("Inserisci Username e Password");

                Console.Write("Username --> ");
                username = Console.ReadLine();

                Console.Write("\nPassword --> ");
                string password = Console.ReadLine();

                try
                {
                    conn.Open();
                    string query1 = "select * from [Utenti] wehre username = @username and password = @password";
                    SqlCommand cmd = new SqlCommand(query1, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows) //se le credenziali inserite sono giuste
                    {
                        reader.Close();
                        Console.WriteLine("ACCESSO CONSENTITO");

                        CaricaListe(id, username, sito, pass);
                        Console.WriteLine("CARICATE");

                        bool esci = false;

                        while (esci == false)
                        {
                            Console.WriteLine("\nCosa vuoi fare?");
                            Console.WriteLine("1-Mostra una password");
                            Console.WriteLine("2-Mostra siti presenti");
                            Console.WriteLine("3-Modifica una password");
                            Console.WriteLine("4-Aggiungi una password");
                            Console.WriteLine("0-Esci");

                            string input = Console.ReadLine();
                            int selezione = int.Parse(input);

                            switch (selezione)
                            {
                                case 0:
                                    {
                                        esci = true;
                                        break;
                                    }

                                case 1:
                                    {
                                        string nomesito;
                                        Console.Write("inserisci il nome del sito da cercare: ");
                                        nomesito = Console.ReadLine();
                                        MostraPsw(nomesito, sito, pass);
                                        break;
                                    }

                                case 2:
                                    {
                                        for (int i = 0; i < sito.Count; i++)
                                        {
                                            Console.WriteLine($"{i + 1}-{sito[i]}");
                                        }
                                        break;
                                    }

                                case 3:
                                    {
                                        Console.WriteLine("Che password vuoi modificare? (indicare il nome del sito)");
                                        selezionesito = Console.ReadLine();

                                        if (sito.Contains(selezionesito))
                                        {
                                            Console.WriteLine("Inserisci la password nuova");
                                            newpass = Console.ReadLine();

                                            if (ModificaPsw(id, newpass, selezionesito, sito, pass) == true)
                                            {
                                                Console.WriteLine($"Password modificata sul sito: {selezionesito}");
                                            }
                                            else
                                            {
                                                Console.WriteLine("Modifca non effettuata");
                                            }
                                        }

                                        else
                                        {
                                            Console.WriteLine($"{selezionesito} inesistente");
                                        }
                                        
                                        break;
                                    }

                                case 4:
                                    {
                                        break;
                                    }
                            }
                        }

                        break;
                    }
                    else
                    {
                        Console.WriteLine("Credenziali non valide");
                        tentativi++;
                    }

                    conn.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            //fine programma
            Console.WriteLine("programma terminato, premere invio ancora");
            Console.ReadLine(); 
        }

        static void CaricaListe(int ID, string username, List<string>sito, List<string> pass) //metodo per caricare le liste
        {
            //passo come parametro solo lo username perchè mi serve solo quello per accedere alle colonne dell'utente
            string query2 = "SELECT Psw.*, Utenti.username, Utenti.password " +
                                        "FROM Psw " +
                                        "INNER JOIN Utenti ON Utenti.ID = Psw.ID AND Utenti.username = @username";

            SqlCommand cmd2 = new SqlCommand(query2, conn);
            cmd2.Parameters.AddWithValue("@username", username);

            SqlDataReader rd = cmd2.ExecuteReader();

            while (rd.Read())
            {
                int ncolonne = rd.FieldCount - 2; //perchè le ultime due colonne sono username e password
                ID = rd.GetInt32(0);

                for (int i = 1; i < ncolonne; i++) //carico nelle liste i valori delle colonne
                {
                    sito.Add(rd.GetName(i)); //ricevo il nome della colonna

                    if (rd.IsDBNull(i)) //controllo se la cella mi ritorna un valore NULL
                    {
                        pass.Add("NULL");
                    }
                    else
                    {
                        pass.Add(rd.GetString(i)); //ricevo il valore del contenuto della cella
                    }
                }
            }

            rd.Close();
        }

        static void MostraPsw(string nomesito, List<string>sito, List<string>pass)
        {
            for(int i = 0; i < sito.Count; i++)
            {
                if (sito[i].ToLower() == nomesito.ToLower()) //metto tutto in minuscolo così evito errori da parte dell'utente
                {
                    Console.WriteLine("password del sito " + sito[i] + ": " + pass[i]);
                }
            }
        }//mostra a video della password di un sito passato come parametro

        static bool ModificaPsw(int id, string newpass, string nomesito, List<string> sito, List<string> pass)
        {
            bool modifica;

            string query = $"UPDATE Psw SET [{nomesito}] = @newpsw WHERE ID = @id";

            SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@newpsw", newpass);
            cmd.Parameters.AddWithValue("@id", id);

            int rowsAffected = cmd.ExecuteNonQuery();

            Console.WriteLine(rowsAffected);

            if(rowsAffected > 0)
            {
                int i = sito.IndexOf(nomesito);
                pass[i] = newpass;
                modifica = true;
            }

            else
            {
                modifica = false;
            }

            return modifica;
        }
    }
}
