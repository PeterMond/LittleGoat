﻿namespace LittleGoat.Controllers
{
    using LittleGoat.DataAccess;
    using LittleGoat.Filter;
    using LittleGoat.Models;
    using LittleGoat.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    public class GameController : BaseController
    {
        private const int NB_SECONDS_TO_SHOW_FIRST_TWO_CARDS = 6;

        [IsAuthenticated]
        public ActionResult Play(string key)
        {
            var playerId = GetPlayerId();

            var model = new PlayViewModel();
            model.SerieId = key;

            model.FirstTwoCards = new List<GameCard>();

            using (LittleGoatEntities entities = new LittleGoatEntities())
            {
                var serie = entities.Serie.FirstOrDefault(p => p.Id == key);
                if(serie == null || !serie.Started)
                {
                    return RedirectToAction("New", "Serie", new { key });
                }

                var currentPlayer = serie.SeriePlayers.FirstOrDefault(p => p.PlayerId == playerId);
                if(currentPlayer == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                var game = entities.Game.Single(p => p.SerieId == key && p.Ended == false);

                model.DateStopShowFirstTwoCards = game.CreationDate.AddSeconds(NB_SECONDS_TO_SHOW_FIRST_TWO_CARDS);

                if (game.CreationDate.AddSeconds(NB_SECONDS_TO_SHOW_FIRST_TWO_CARDS) >= DateTime.UtcNow)
                {
                    model.FirstTwoCards = entities.GameCard.Where(p => p.PlayerId == playerId).OrderBy(p => p.Position).Take(2).ToList();
                }

                model.CardFromDeck = entities.GameCard
                        .Where(p => p.GameId == game.Id && p.PlayerId == null)
                        .OrderBy(p => p.Position)
                        .FirstOrDefault();

                model.NextExpectedAction = string.Format(Resources.next_to_play_1, game.Player2.Name);

                model.LastChatMessages = entities.SerieChat
                    .Where(p => p.SerieId == key)
                    .OrderByDescending(p => p.Id)
                    .Select(p => new ChatMessage() { Id = p.Id, Date = p.Date, Message = p.Message, PlayerId = p.PlayerId, PlayerName = p.Player.Name })
                    .Take(30)
                    .OrderBy(p => p.Date)
                    .ToList();
            }

            return View(model);
        }

        public ActionResult SwitchPlayerCards(int firstCard, int secondCard)
        {
            throw new NotImplementedException();
        }
    }
}