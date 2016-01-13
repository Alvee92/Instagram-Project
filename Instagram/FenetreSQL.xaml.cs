using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using MySql.Data;
using System.Xml;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;

namespace Instagram
{
    /// <summary>
    /// Logique d'interaction pour FenetreSQL.xaml
    /// </summary>
    public partial class FenetreSQL : Window
    {
        #region Variblaes Gobales

        private static int compteurSql = 0;  //compte le nombre de média entrés dans la base
        private static int placemedia = 0;   //Sert à avoir la plaace du media qu'on va ajouter
        private static bool _quitter = false; //permettra de savoir quand les threads devront s'arreter
        private static List<InstaSharp.Model.Media> listefinale = new List<InstaSharp.Model.Media>(); //la liste de tout les medias lu
        private static DateTime nx = new DateTime(1970, 1, 1);  //temps a soustraitre
        private static TimeSpan ts = DateTime.UtcNow - nx;  //temps actuel en unix
        private static string time = System.IO.File.ReadAllText("time.txt");  //on lit le dernier temps enregistré dans le fichier texte
        private static MySqlConnection connection;  //pour se connecter à la base
        private static string connectionString;     //contient les info relative à l abase
        Thread threadRecherche = new Thread(RechercheMedia);  //thread pour la cherche sur l'API
        Thread threadSql = new Thread(Sql);                   //thread pour l'insertion dans la base
        private static Stopwatch TempsTotal = new Stopwatch();

        #endregion
        

        public FenetreSQL()
        {
            InitializeComponent();
        }

        private void validation_Click(object sender, RoutedEventArgs e)
        {
            TempsTotal.Start(); //On commence à compter à partir de l'execution
            //On met quitter a false pour pouvoir redémarer les threads
            _quitter = false;

            //On recupere les info des textbox et on crée un string de connection au serveur
            connectionString = "SERVER=" + serveur.Text + ";DATABASE=" + database.Text + ";UID=" + login.Text + ";PASSWORD=" + pass.Text + ";";

            //On démare les threads
            try
            {
                MessageBox.Show("Recherche démarée");
                threadRecherche.Start();
                threadSql.Start();
            }
            catch (Exception ex) { MessageBox.Show("Rechrche déjà en cours !"); }
          
        }

        public static void Sql()
        {
            
            Thread.Sleep(2000); //Le thread à besoin d'attendre que la liste finale se remplisse

            #region Connexion

            connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open(); //Ouverture de la connection
            }

            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show("Impossible de se conncter, verifiez les parametres de la connexion");
            }

            #endregion

            while (!_quitter)
            {
                InstaSharp.Model.Media m = new InstaSharp.Model.Media();
                try
                {
                    m = listefinale[placemedia]; //on choisit le media dans la liste
                }
                catch (Exception el)
                { }
                string requete;    //va contenir la requete
                MySqlCommand command;  //variable pour éxécuter la commande

                try
                {
                    //ici on rentre les users
                    requete = "INSERT INTO `isngr9`.`user` (`idUser`, `username`, `fullname`, `profilPicture`) VALUES ('" + m.User.Id + "', '" + m.User.Username + "', '" + m.User.FullName + "','" + m.User.ProfilePicture + "');";
                    command = new MySqlCommand(requete, connection);
                    command.ExecuteNonQuery();
                    compteurSql++;
                }
                catch (Exception ex) { }



                try
                {
                    if (m.Location.Name != null) // on teste si la location possède une place (un nom et un id, car sinon ceux ci sont nul et on à une erreur
                    {

                        try
                        {
                            requete = "INSERT INTO `isngr9`.`media` (`idMedia`, `idUser`, `createdtime`, `type`, `like`, `comment`) VALUES ('" + m.Id + "', '" + m.User.Id + "', '" + m.CreatedTime + "', '" + m.Type + "', '" + m.Likes.Count + "', '" + m.Comments.Count + "');";
                            //MessageBox.Show(requete);
                            command = new MySqlCommand(requete, connection);
                            System.IO.File.WriteAllText("time.txt", "&max_timestamp=" + m.CreatedTime); //On écrit dans un fichier texte le temps de la dernière photo entré, pour reprendre en cas de crash;
                            command.ExecuteNonQuery();

                            requete = "INSERT INTO `isngr9`.`location` (`idLoc`, `latitude`, `idLieu`, `longitude`, `name`,`idMedia`) VALUES (NULL, '" + m.Location.Latitude + "', '" + m.Location.Id + "', '" + m.Location.Longitude + "', '" + m.Location.Name + "','" + m.Id + "');";
                            //MessageBox.Show(requete);
                            command = new MySqlCommand(requete, connection);
                            command.ExecuteNonQuery();


                        }
                        catch (Exception ex) { }

                    }

                    else
                    {
                        try
                        {
                            requete = "INSERT INTO `isngr9`.`media` (`idMedia`, `idUser`, `createdtime`, `type`, `like`, `comment`) VALUES ('" + m.Id + "' , '" + m.User.Id + "', '" + m.CreatedTime + "', '" + m.Type + "', '" + m.Likes.Count + "', '" + m.Comments.Count + "');";
                            command = new MySqlCommand(requete, connection);
                            command.ExecuteNonQuery();
                            System.IO.File.WriteAllText("time.txt", "&max_timestamp=" + m.CreatedTime);


                            requete = "INSERT INTO `isngr9`.`location` (`idLoc`, `idLieu`, `latitude`, `longitude`, `name`, `idMedia`) VALUES (NULL, '0', '" + m.Location.Latitude + "', '" + m.Location.Longitude + "', 'null','" + m.Id + "');";
                            command = new MySqlCommand(requete, connection);
                            command.ExecuteNonQuery();


                        }

                        catch (Exception ex) { }



                    }
                    placemedia++;  //on va au media suivant
                }
                catch (Exception el) { }

            }
           
        }



        public static void RechercheMedia()
        {
            string reponse;
            

            while (!_quitter) //ici i représente le nombre de boucle que l'on va faire avec la requete, chaque boucle donne a peu près 20 photos
            {
                try
                {
                    reponse = InstaSharp.HttpClient.GET("https://api.instagram.com/v1/media/search?lat=45.759723&lng=4.842223&distance=5000&client_id=7c0d9cfdaf3849b7864ebf2f6f82ff75" + time); //la requete de l'api instagram


                    /////On convertir la reponse en XMl
                    XmlDocument doc = (XmlDocument)JsonConvert.DeserializeXmlNode(reponse, "root");
                    doc.Save("testxml.xml");



                    List<InstaSharp.Model.Media> liste = Outils.lirexml("testxml.xml"); //méthode qui lit le fichier Xml et crée les classe adéquat dans une liste
                    time = "&max_timestamp=" + liste[liste.Count - 1].CreatedTime; //on récupere le created time du dernier media de cette liste

                    listefinale.AddRange(liste); //on ajoute cette liste de 20 medias à notre liste finale qui contiendra tous les medias
                }
                catch (Exception e) { }
            }
            
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            TempsTotal.Stop();
            TimeSpan ts = TempsTotal.Elapsed;


            ts.ToString("mm\\ss");
            
            _quitter = true;   //On me quitter a true pour arreter les threads
            
           
            MessageBox.Show("L'éxécution à duré " + ts + " minutes \n "+ "En tout "  + placemedia + " medias ont été insérés");
            this.Close();


        }



        private void ReinitialiserTemps(object sender, RoutedEventArgs e) //Sert a mettre le timestamp au temps actuel (unix)
        {
            
            string time = "&max_timestamp=" + ((int)ts.TotalSeconds).ToString();
            MessageBox.Show("Le temps à été réinitialisé!");
            System.IO.File.WriteAllText("time.txt", time); //On écrit dans un fichier texte le temps de la dernière photo entré, pour reprendre en cas de crash;


        }

      
    }
}
