using System;
using System.Collections.Generic;
using System.Linq;
using AndNetwork.Shared;
using AndNetwork.Shared.Enums;
using Discord.Commands;

namespace AndNetwork.Server.Discord.Utility
{
    public static class DiscordExtensions
    {
        public static string GetLocalizedString(this CommandError commandError) => commandError switch
        {
            CommandError.UnknownCommand => "Неизвестная команда",
            CommandError.ParseFailed => "Невеный формат аргументов",
            CommandError.BadArgCount => "Неверное количество аргументов",
            CommandError.ObjectNotFound => "Объект не найден",
            CommandError.MultipleMatches => "Неоднозначный вызов команды",
            CommandError.UnmetPrecondition => "Доступ запрещен",
            CommandError.Exception => "Ошибка при выполнении команды",
            CommandError.Unsuccessful => "Команда не была успешно выполнена",
            _ => throw new ArgumentOutOfRangeException(nameof(commandError), commandError, null),
        };

        public static string GetAwardSymbol(this ClanAwardTypeEnum awardType) => awardType switch
        {
            ClanAwardTypeEnum.None => string.Empty,
            ClanAwardTypeEnum.Bronze => "\U0001F7EB",
            ClanAwardTypeEnum.Silver => "\U00002B1C",
            ClanAwardTypeEnum.Gold => "\U0001F7E8",
            ClanAwardTypeEnum.Hero => "\U0001F7E6",
            _ => throw new ArgumentOutOfRangeException(nameof(awardType), awardType, null),
        };

        public static IEnumerable<string> GetStrings(this IEnumerable<ClanAward> awards) => awards.OrderBy(x => x).Select(x => string.IsNullOrWhiteSpace(x.Description) ? $"{x.Type.GetAwardSymbol()} [{x.Date:d}]" : $"{x.Type.GetAwardSymbol()} [{x.Date:d}]: {x.Description}");
    }
}
