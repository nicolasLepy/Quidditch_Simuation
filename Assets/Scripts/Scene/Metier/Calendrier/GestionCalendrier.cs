﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class GestionCalendrier
{

    public static List<Match> GenererCalendrier(List<Club> clubs, List<DateTime> dates, DateTime heure, List<Decalage> decalages)
    {
        List<Match> res = new List<Match>();
        bool ghost = false;
        int teams = clubs.Count;
        if (teams % 2 == 1)
        {
            ghost = true;
            teams++;
        }

        int totalRound = teams - 1;
        int matchsPerRound = teams / 2;
        string[,] rounds = new string[totalRound, matchsPerRound];

        for (int round = 0; round < totalRound; round++)
        {
            for (int match = 0; match < matchsPerRound; match++)
            {
                int home = (round + match) % (teams - 1);
                int away = (teams - 1 - match + round) % (teams - 1);

                if (match == 0)
                    away = teams - 1;

                rounds[round, match] = (home + 1) + " v " + (away + 1);

            }
        }

        string[,] interleaved = new string[totalRound, matchsPerRound];
        int evn = 0;
        int odd = (teams / 2);

        for (int i = 0; i < totalRound; i++)
        {
            if (i % 2 == 0)
            {
                for (int j = 0; j < matchsPerRound; j++)
                {
                    interleaved[i, j] = rounds[evn, j];
                }
                evn++;
            }
            else
            {
                for (int j = 0; j < matchsPerRound; j++)
                {
                    interleaved[i, j] = rounds[odd, j];
                }
                odd++;
            }
        }

        rounds = interleaved;

        for (int round = 0; round < totalRound; round++)
        {
            if (round % 2 == 1)
            {
                rounds[round, 0] = flip(rounds[round, 0]);
            }
        }

        for (int i = 0; i < totalRound; i++)
        {
            List<Match> matchs = new List<Match>();
            for (int j = 0; j < matchsPerRound; j++)
            {
                
                string[] compenents = rounds[i, j].Split(new string[] { " v " }, StringSplitOptions.None);
                int a = int.Parse(compenents[0]);
                int b = int.Parse(compenents[1]);

                if (!ghost || (a != teams && b != teams))
                {

                    //-1 car index dans liste de 0 à n-1, et des clubs de 1 à n
                    Club e1 = clubs[a - 1];
                    Club e2 = clubs[b - 1];

                    //Date du match
                    DateTime jour = new DateTime(Session.Instance.Partie.Date.Year, dates[i].Month, dates[i].Day, heure.Hour, heure.Minute, 0);
                    if (jour.Month < 7) jour = jour.AddYears(1);
                    Match m = new Match(e1, e2, jour);
                    res.Add(m);
                    matchs.Add(m);
                }
            }
            Programme(matchs, decalages);
        }

        return res;
    }

    private static string flip(string match)
    {
        string[] components = match.Split(new string[] { " v " }, StringSplitOptions.None);
        return components[1] + " v " + components[0];
    }

    private static void Programme(List<Match> matchs, List<Decalage> decalages)
    {
        int indice = 0;
        matchs.Sort(new MatchNiveau_Comparator());
        foreach(Decalage d in decalages)
        {
            for(int i = 0; i <d.NombreMatchs; i++)
            {
                TimeSpan ts = new TimeSpan(d.Heure.Hour, d.Heure.Minute, 0);
                matchs[indice].Jour = matchs[indice].Jour.Date + ts;
                matchs[indice].Jour = matchs[indice].Jour.AddDays(d.DecalageJours);
                indice++;
            }
        }
    }

    public static List<Match> TirageAuSort(TourElimination tour)
    {
        List<Match> res = new List<Match>();
        List<Club> pot = new List<Club>(tour.Clubs);
        for(int i = 0; i<tour.Clubs.Count/2; i++)
        {
            Club dom = TirerClub(pot);
            Club ext = TirerClub(pot);
            DateTime jour = new DateTime(Session.Instance.Partie.Date.Year, tour.DateMatchs[0].Month, tour.DateMatchs[0].Day, tour.Heure.Hour, tour.Heure.Minute, 0);
            if (jour.Month < 7) jour = jour.AddYears(1);

            res.Add(new Match(dom, ext, jour));
        }

        Programme(res, tour.Decalages);

        return res;
    }

    private static Club TirerClub(List<Club> pot)
    {
        Club res = pot[UnityEngine.Random.Range(0,pot.Count-1)];
        pot.Remove(res);
        return res;
    }

}