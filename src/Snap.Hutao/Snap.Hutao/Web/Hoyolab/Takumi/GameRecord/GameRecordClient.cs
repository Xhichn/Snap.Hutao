﻿// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Extension;
using Snap.Hutao.Service.Abstraction;
using Snap.Hutao.Web.Hoyolab.DynamicSecret;
using Snap.Hutao.Web.Hoyolab.Takumi.GameRecord.Avatar;
using Snap.Hutao.Web.Response;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Snap.Hutao.Web.Hoyolab.Takumi.GameRecord;

/// <summary>
/// 游戏记录提供器
/// </summary>
[Injection(InjectAs.Transient)]
internal class GameRecordClient
{
    private readonly IUserService userService;
    private readonly HttpClient httpClient;
    private readonly JsonSerializerOptions jsonSerializerOptions;

    /// <summary>
    /// 构造一个新的游戏记录提供器
    /// </summary>
    /// <param name="userService">用户服务</param>
    /// <param name="httpClient">请求器</param>
    public GameRecordClient(IUserService userService, HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions)
    {
        this.userService = userService;
        this.httpClient = httpClient;
        this.jsonSerializerOptions = jsonSerializerOptions;
    }

    /// <summary>
    /// 获取玩家基础信息
    /// </summary>
    /// <param name="uid">uid</param>
    /// <param name="token">取消令牌</param>
    /// <returns>玩家的基础信息</returns>
    public async Task<PlayerInfo?> GetPlayerInfoAsync(PlayerUid uid, CancellationToken token)
    {
        string url = string.Format(ApiEndpoints.GameRecordIndex, uid.Value, uid.Region);

        Response<PlayerInfo>? resp = await httpClient
            .SetUser(userService.Current)
            .UsingDynamicSecret2(jsonSerializerOptions, url)
            .GetFromJsonAsync<Response<PlayerInfo>>(url, jsonSerializerOptions, token)
            .ConfigureAwait(false);

        return resp?.Data;
    }

    /// <summary>
    /// 获取玩家深渊信息
    /// </summary>
    /// <param name="uid">uid</param>
    /// <param name="schedule">1：当期，2：上期</param>
    /// <param name="token">取消令牌</param>
    /// <returns>深渊信息</returns>
    public async Task<SpiralAbyss.SpiralAbyss?> GetSpiralAbyssAsync(PlayerUid uid, SpiralAbyssSchedule schedule, CancellationToken token = default)
    {
        string url = string.Format(ApiEndpoints.SpiralAbyss, (int)schedule, uid.Value, uid.Region);

        Response<SpiralAbyss.SpiralAbyss>? resp = await httpClient
            .SetUser(userService.Current)
            .UsingDynamicSecret2(jsonSerializerOptions, url)
            .GetFromJsonAsync<Response<SpiralAbyss.SpiralAbyss>>(url, jsonSerializerOptions, token)
            .ConfigureAwait(false);

        return resp?.Data;
    }

    /// <summary>
    /// 获取玩家角色详细信息
    /// </summary>
    /// <param name="uid">uid</param>
    /// <param name="playerInfo">玩家的基础信息</param>
    /// <param name="token">取消令牌</param>
    /// <returns>角色列表</returns>
    public async Task<List<Character>> GetCharactersAsync(PlayerUid uid, PlayerInfo playerInfo, CancellationToken token = default)
    {
        CharacterData data = new(uid, playerInfo.Avatars.Select(x => x.Id));

        HttpResponseMessage? response = await httpClient
            .SetUser(userService.Current)
            .UsingDynamicSecret2(jsonSerializerOptions, ApiEndpoints.Character, data)
            .PostAsJsonAsync(ApiEndpoints.Character, data, token)
            .ConfigureAwait(false);

        Response<CharacterWrapper>? resp = await response.Content.ReadFromJsonAsync<Response<CharacterWrapper>>(jsonSerializerOptions, token);

        return EnumerableExtensions.EmptyIfNull(resp?.Data?.Avatars);
    }

    private class CharacterData
    {
        public CharacterData(PlayerUid uid, IEnumerable<int> characterIds)
        {
            CharacterIds = characterIds;
            Uid = uid.Value;
            Server = uid.Region;
        }

        [JsonPropertyName("character_ids")]
        public IEnumerable<int> CharacterIds { get; }

        [JsonPropertyName("role_id")]
        public string Uid { get; }

        [JsonPropertyName("server")]
        public string Server { get; }
    }
}