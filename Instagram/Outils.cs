using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
namespace Instagram
{
    public class Outils
    {



        public static List<InstaSharp.Model.Media> lirexml(string fichier)
        {
            List<InstaSharp.Model.Media> listmedia = new List<InstaSharp.Model.Media>();
            XmlDocument doc = new XmlDocument();
            doc.Load(fichier);
            XmlNode noeud = doc.FirstChild.FirstChild;

            while (noeud != null)
            {
                switch (noeud.Name)
                {
                    case "data":
                        InstaSharp.Model.Media media = new InstaSharp.Model.Media();
                        List<InstaSharp.Model.Tag> listtag = new List<InstaSharp.Model.Tag>();
                        media.Tags = listtag;
                        int compteurenfant = 0;
                        while (compteurenfant < noeud.ChildNodes.Count)
                        {


                            try
                            {
                                //string s = noeud.FirstChild.NextSibling.Name;

                                switch (noeud.ChildNodes[compteurenfant].Name)
                                {

                                    #region Location
                                    case "location":



                                        InstaSharp.Model.Location location = new InstaSharp.Model.Location();
                                        if (noeud.ChildNodes[compteurenfant].ChildNodes.Count == 4)
                                        {

                                            location.Latitude = noeud.ChildNodes[compteurenfant].ChildNodes[0].InnerText;
                                            location.Name = noeud.ChildNodes[compteurenfant].ChildNodes[1].InnerText.Replace("'", "''");
                                            location.Longitude = noeud.ChildNodes[compteurenfant].ChildNodes[2].InnerText;
                                            location.Id = Convert.ToInt32(noeud.ChildNodes[compteurenfant].ChildNodes[3].InnerText);
                                            media.Location = location;
                                        }
                                        else
                                        {


                                            location.Latitude = noeud.ChildNodes[compteurenfant].ChildNodes[0].InnerText;
                                            location.Longitude = noeud.ChildNodes[compteurenfant].ChildNodes[1].InnerText;
                                            media.Location = location;
                                        }
                                        break;
                                    #endregion

                                    #region Tag

                                    case "tags":
                                        InstaSharp.Model.Tag t = new InstaSharp.Model.Tag();
                                        t.Name = noeud.ChildNodes[compteurenfant].InnerText;
                                        listtag.Add(t);
                                        media.Tags = listtag;
                                        string s = media.Tags.Count.ToString();
                                        break;
                                    #endregion


                                    #region Time
                                    case "created_time":

                                        media.CreatedTime = noeud.ChildNodes[compteurenfant].InnerText;


                                        break;
                                    #endregion

                                    #region Id
                                    case "id":

                                        media.Id = noeud.ChildNodes[compteurenfant].InnerText;


                                        break;
                                    #endregion 

                                    #region Like
                                    case "likes":

                                        InstaSharp.Model.Like like = new InstaSharp.Model.Like();
                                        like.Count = noeud.ChildNodes[compteurenfant].ChildNodes[0].InnerText;
                                        media.Likes = like;


                                        break;
                                    #endregion

                                    #region Coomments

                                    case "comments":
                                        InstaSharp.Model.Comments comments = new InstaSharp.Model.Comments();
                                        comments.Count = noeud.ChildNodes[compteurenfant].ChildNodes[0].InnerText;
                                        media.Comments = comments;


                                        break;
                                    #endregion

                                    #region Type
                                    case "type":

                                        media.Type = noeud.ChildNodes[compteurenfant].InnerText;


                                        break;
                                    #endregion

                                    

                                    #region User
                                    case "user":

                                        InstaSharp.Model.User user = new InstaSharp.Model.User();
                                        user.Username = noeud.ChildNodes[compteurenfant].ChildNodes[0].InnerText.Replace("'", "''");
                                        user.ProfilePicture = noeud.ChildNodes[compteurenfant].ChildNodes[1].InnerText;
                                        user.Id = noeud.ChildNodes[compteurenfant].ChildNodes[2].InnerText;
                                        user.FullName = noeud.ChildNodes[compteurenfant].ChildNodes[3].InnerText.Replace("'", "''");

                                        media.User = user;


                                        break;

                                    #endregion

                                }
                            }
                            catch (Exception e)
                            {

                            }

                            compteurenfant++;

                        }


                        listmedia.Add(media);
                        break;
                       
                }
                noeud = noeud.NextSibling;
            }
          
           // XmlNodeList elements = doc.DocumentElement.SelecSingletNodes("//data");

            return listmedia;
        }

       
  
               
        }

  }

