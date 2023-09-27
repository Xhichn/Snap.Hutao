// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using Snap.Hutao.Core.DependencyInjection.Annotation.HttpClient;
using Snap.Hutao.Web.Hoyolab.Annotation;
using Snap.Hutao.Web.Hoyolab.DynamicSecret;
using Snap.Hutao.Web.Response;
using System.Net.Http;
using System.Net.Http.Json;

namespace Snap.Hutao.Web.Hoyolab.Passport;

/// <summary>
/// 通行证客户端
/// </summary>
[UseDynamicSecret]
[ConstructorGenerated(ResolveHttpClient = true)]
[HttpClient(HttpClientConfiguration.XRpc4)] // TODO: New client type
internal sealed partial class PassportClient2Oversea : IPassportClient2
{
    private readonly JsonSerializerOptions options;
    private readonly HttpClient httpClient;

    /// <summary>
    /// V1 SToken 登录
    /// </summary>
    /// <param name="stokenV1">v1 SToken</param>
    /// <param name="token">取消令牌</param>
    /// <returns>登录数据</returns>
    [ApiInformation(Salt = SaltType.OSPROD)]
    public async ValueTask<Response<LoginResult>> LoginBySTokenAsync(Cookie stokenV1, CancellationToken token = default)
    {
        HttpResponseMessage message = await httpClient
            .SetHeader("Cookie", stokenV1.ToString())
            .UseDynamicSecret(DynamicSecretVersion.Gen2, SaltType.OSPROD, true)
            .PostAsync(ApiOsEndpoints.AccountGetSTokenByOldToken, null, token)
            .ConfigureAwait(false);

        Response<LoginResult>? resp = await message.Content
            .ReadFromJsonAsync<Response<LoginResult>>(options, token)
            .ConfigureAwait(false);

        return Response.Response.DefaultIfNull(resp);
    }

    private class Timestamp
    {
        [JsonPropertyName("t")]
        public long Time { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
