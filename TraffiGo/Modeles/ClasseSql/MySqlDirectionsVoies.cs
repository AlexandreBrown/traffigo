﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraffiGo.Modeles.Data;

namespace TraffiGo.Modeles.ClasseSql
{
    class MySqlDirectionsVoies
    {
        public static MySqlConnexion ConnectionBD { get; private set; }

        public static List<Direction> Retrieve(Voie v, string nom, string auteur, Chemin c, Route r)
        {

            StringBuilder query = new StringBuilder();

            if (c.Emplacement != null)
            {
                query.Append("SELECT * FROM DirectionsVoies WHERE idVoie = (SELECT idVoie FROM Voies WHERE idRoute = (SELECT idRoute FROM Routes WHERE idChemin = (SELECT idChemin FROM Chemins WHERE idSimulation = (SELECT idSimulation FROM Simulations WHERE nom = '" + MySqlHelper.EscapeString(nom) + "' AND idUtilisateur = (SELECT idUtilisateur FROM Utilisateurs WHERE nomUtilisateur = '" + MySqlHelper.EscapeString(auteur) + "')) AND  posX = '" + (int)(c.Position.X) + "' AND posY = '" + (int)(c.Position.Y) + "' AND idEmplacement = '" + (int)c.Emplacement + "') AND idOrientation = '" + (int)(r.Orientation) + "'))");
            }
            else
            {
                query.Append("SELECT * FROM DirectionsVoies WHERE idVoie = (SELECT idVoie FROM Voies WHERE idRoute = (SELECT idRoute FROM Routes WHERE idChemin = (SELECT idChemin FROM Chemins WHERE idSimulation = (SELECT idSimulation FROM Simulations WHERE nom = '" + MySqlHelper.EscapeString(nom) + "' AND idUtilisateur = (SELECT idUtilisateur FROM Utilisateurs WHERE nomUtilisateur = '" + MySqlHelper.EscapeString(auteur) + "')) AND  posX = '" + (int)(c.Position.X) + "' AND posY = '" + (int)(c.Position.Y) + "' AND idEmplacement IS NULL) AND idOrientation = '" + (int)(r.Orientation) + "'))");
            }

            


            return RetrieveListeDirectionsVoies(query.ToString(), nom, auteur, c);
        }

        public static List<Direction> RetrieveListeDirectionsVoies(string query, string nom, string auteur, Chemin c)
        {
            List<Direction> lstResultat = new List<Direction>();
            DataSet dsResultat;
            DataTable dtResultat;

            ConnectionBD = new MySqlConnexion();

            dsResultat = ConnectionBD.Query(query);
            dtResultat = dsResultat.Tables[0];

            foreach (DataRow dr in dtResultat.Rows)
            {
                lstResultat.Add((Direction)dr["idDirection"]
                               );
            }

            return lstResultat;
        }

        public static void InsertSansIntersection(Simulation simulation, Route r, Chemin c, Direction dv)
        {
            StringBuilder nonquery = new StringBuilder();
            ConnectionBD = new MySqlConnexion();
            Utilisateur u = MySqlUtilisateurs.Retrieve(simulation.Auteur);

            try
            {
                nonquery = new StringBuilder();

                nonquery.Append("INSERT INTO DirectionsVoies (idVoie, idDirection) VALUES ")
                                .Append("(").Append("(SELECT idVoie FROM Voies WHERE idRoute = (SELECT idRoute FROM Routes WHERE idChemin = (SELECT idChemin FROM Chemins WHERE idSimulation = (SELECT idSimulation FROM Simulations WHERE nom = '" + MySqlHelper.EscapeString(simulation.Nom) + "' AND idUtilisateur = (SELECT idUtilisateur FROM Utilisateurs WHERE nomUtilisateur = '" + MySqlHelper.EscapeString(simulation.Auteur) + "')) AND  posX = '" + (int)(c.Position.X) + "' AND posY = '" + (int)(c.Position.Y) + "') AND idOrientation = '" + (int)(r.Orientation) + "'))")
                                .Append(",'").Append((int)dv).Append("')");

                ConnectionBD.NonQuery(nonquery.ToString());
            }
            catch (MySqlException e)
            {
                Exception error;
                switch (e.Number)
                {
                    case 1040:
                        error = new Exception("Le serveur reçoit trop de connexions simultanées, veuillez réessayer dans quelques instants. ");
                        break;
                    case 1062:
                        if (e.Message.Contains(simulation.Nom))
                        {
                            error = new Exception("Ce nom de simulation est déjà utilisé. ");
                        }
                        else
                        {
                            error = new Exception("Impossible de sauvegarder la simulation. ");

                        }
                        break;
                    default:
                        error = new Exception("Connexion au serveur impossible , veuillez réessayer plus tard. "); ;
                        break;
                }

                throw error;
            }
        }

        public static void InsertAvecIntersection(Simulation simulation, Route r, Chemin c, Direction dv, int posX, int posY)
        {
            StringBuilder nonquery = new StringBuilder();
            ConnectionBD = new MySqlConnexion();
            Utilisateur u = MySqlUtilisateurs.Retrieve(simulation.Auteur);

            try
            {
                nonquery = new StringBuilder();

                nonquery.Append("INSERT INTO DirectionsVoies (idVoie, idDirection) VALUES ")
                                .Append("(").Append("(SELECT idVoie FROM Voies WHERE idRoute = (SELECT idRoute FROM Routes WHERE idChemin = (SELECT idChemin FROM Chemins WHERE idSimulation = (SELECT idSimulation FROM Simulations WHERE nom = '" + MySqlHelper.EscapeString(simulation.Nom) + "' AND idUtilisateur = (SELECT idUtilisateur FROM Utilisateurs WHERE nomUtilisateur = '" + MySqlHelper.EscapeString(simulation.Auteur) + "')) AND idIntersection = (SELECT idIntersection FROM Intersections WHERE idSimulation = (SELECT idSimulation FROM Simulations WHERE nom = '" + MySqlHelper.EscapeString(simulation.Nom) + "' AND idUtilisateur = (SELECT idUtilisateur FROM Utilisateurs WHERE nomUtilisateur = '" + MySqlHelper.EscapeString(simulation.Auteur) + "')) AND posX = '" + posX + "' AND posY = '" + posY + "') AND  posX = '" + (int)(c.Position.X) + "' AND posY = '" + (int)(c.Position.Y) + "' AND idEmplacement = '" + (int)c.Emplacement + "') AND idOrientation = '" + (int)(r.Orientation) + "'))")
                                .Append(",'").Append((int)dv).Append("')");

                ConnectionBD.NonQuery(nonquery.ToString());
            }
            catch (MySqlException e)
            {
                Exception error;
                switch (e.Number)
                {
                    case 1040:
                        error = new Exception("Le serveur reçoit trop de connexions simultanées, veuillez réessayer dans quelques instants. ");
                        break;
                    case 1062:
                        if (e.Message.Contains(simulation.Nom))
                        {
                            error = new Exception("Ce nom de simulation est déjà utilisé. ");
                        }
                        else
                        {
                            error = new Exception("Impossible de sauvegarder la simulation. ");

                        }
                        break;
                    default:
                        error = new Exception("Connexion au serveur impossible , veuillez réessayer plus tard. "); ;
                        break;
                }

                throw error;
            }
        }
    }
}
