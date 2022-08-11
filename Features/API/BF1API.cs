using BF1.ServerAdminTools.Common.Data;
using BF1.ServerAdminTools.Common.Utils;

using RestSharp;

namespace BF1.ServerAdminTools.Features.API;

public static class BF1API
{
    private const string Host = "https://sparta-gw.battlelog.com/jsonrpc/pc/api";

    private static RestClient client;
    private static Dictionary<string, string> headers;

    /// <summary>
    /// 初始化RestSharp
    /// </summary>
    public static void Init()
    {
        if (client == null)
        {
            var options = new RestClientOptions(Host)
            {
                MaxTimeout = 5000
            };

            client = new RestClient(options);

            headers = new Dictionary<string, string>();
            headers["User-Agent"] = "ProtoHttp 1.3/DS 15.1.2.1.0 (Windows)";
            headers["X-GatewaySession"] = Globals.SessionId;
            headers["X-ClientVersion"] = "release-bf1-lsu35_26385_ad7bf56a_tunguska_all_prod";
            headers["X-DbId"] = "Tunguska.Shipping2PC.Win32";
            headers["X-CodeCL"] = "3779779";
            headers["X-DataCL"] = "3779779";
            headers["X-SaveGameVersion"] = "26";
            headers["X-HostingGameId"] = "tunguska";
            headers["X-Sparta-Info"] = "tenancyRootEnv=unknown; tenancyBlazeEnv=unknown";
        }
    }

    /// <summary>
    /// 获取战地1欢迎语
    /// </summary>
    public static async Task<RespContent> GetWelcomeMessage()
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "Onboarding.welcomeMessage",
                @params = new
                {
                    game = "tunguska",
                    minutesToUTC = "-480"
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 设置API语言
    /// </summary>
    public static async Task<RespContent> SetAPILocale()
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "CompanionSettings.setLocale",
                @params = new
                {
                    locale = "zh_TW"
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 踢出指定玩家
    /// </summary>
    public static async Task<RespContent> AdminKickPlayer(string personaId, string reason)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.kickPlayer",
                @params = new
                {
                    game = "tunguska",
                    gameId = Globals.GameId,
                    personaId = personaId,
                    reason = reason
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 更换指定玩家队伍
    /// </summary>
    public static async Task<RespContent> AdminMovePlayer(string personaId, string teamId)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.movePlayer",
                @params = new
                {
                    game = "tunguska",
                    personaId = personaId,
                    gameId = Globals.GameId,
                    teamId = teamId,
                    forceKill = true,
                    moveParty = false
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 更换服务器地图
    /// </summary>
    public static async Task<RespContent> ChangeServerMap(string persistedGameId, string levelIndex)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.chooseLevel",
                @params = new
                {
                    game = "tunguska",
                    persistedGameId = persistedGameId,
                    levelIndex = levelIndex
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 获取完整服务器详情
    /// </summary>
    public static async Task<RespContent> GetFullServerDetails()
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "GameServer.getFullServerDetails",
                @params = new
                {
                    game = "tunguska",
                    gameId = Globals.GameId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 添加服务器管理员
    /// </summary>
    public static async Task<RespContent> AddServerAdmin(string personaName)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.addServerAdmin",
                @params = new
                {
                    game = "tunguska",
                    serverId = Globals.ServerId,
                    personaName = personaName
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 移除服务器管理员
    /// </summary>
    public static async Task<RespContent> RemoveServerAdmin(string personaId)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.removeServerAdmin",
                @params = new
                {
                    game = "tunguska",
                    serverId = Globals.ServerId,
                    personaId = personaId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 添加服务器VIP
    /// </summary>
    public static async Task<RespContent> AddServerVip(string personaName)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.addServerVip",
                @params = new
                {
                    game = "tunguska",
                    serverId = Globals.ServerId,
                    personaName = personaName
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 移除服务器VIP
    /// </summary>
    public static async Task<RespContent> RemoveServerVip(string personaId)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.removeServerVip",
                @params = new
                {
                    game = "tunguska",
                    serverId = Globals.ServerId,
                    personaId = personaId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 添加服务器BAN
    /// </summary>
    public static async Task<RespContent> AddServerBan(string personaName)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.addServerBan",
                @params = new
                {
                    game = "tunguska",
                    serverId = Globals.ServerId,
                    personaName = personaName
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 移除服务器BAN
    /// </summary>
    public static async Task<RespContent> RemoveServerBan(string personaId)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.removeServerBan",
                @params = new
                {
                    game = "tunguska",
                    serverId = Globals.ServerId,
                    personaId = personaId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 获取服务器RSP信息
    /// </summary>
    public static async Task<RespContent> GetServerDetails()
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.getServerDetails",
                @params = new
                {
                    game = "tunguska",
                    serverId = Globals.ServerId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 更新服务器信息
    /// </summary>
    public static async Task<RespContent> UpdateServer(UpdateServerReqBody reqBody)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 搜索服务器
    /// </summary>
    /// <param name="serverName"></param>
    /// <returns></returns>
    public static async Task<RespContent> SearchServers(string serverName)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "GameServer.searchServers",
                @params = new
                {
                    filterJson = "{\"version\":6,\"name\":\"" + serverName + "\"}",
                    game = "tunguska",
                    limit = 20,
                    protocolVersion = "3779779"
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 离开当前服务器
    /// </summary>
    /// <returns></returns>
    public static async Task<RespContent> LeaveGame()
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "Game.leaveGame",
                @params = new
                {
                    game = "tunguska",
                    gameId = Globals.GameId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 获取玩家PersonasByIds
    /// </summary>
    /// <returns></returns>
    public static async Task<RespContent> GetPersonasByIds(string personaId)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "RSP.getPersonasByIds",
                @params = new
                {
                    game = "tunguska",
                    personaIds = new[] { personaId }
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 获取玩家战绩
    /// </summary>
    public static async Task<RespContent> DetailedStatsByPersonaId(string personaId)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "Stats.detailedStatsByPersonaId",
                @params = new
                {
                    game = "tunguska",
                    personaId = personaId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 获取玩家武器
    /// </summary>
    public static async Task<RespContent> GetWeaponsByPersonaId(string personaId)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "Progression.getWeaponsByPersonaId",
                @params = new
                {
                    game = "tunguska",
                    personaId = personaId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }

    /// <summary>
    /// 获取玩家载具
    /// </summary>
    public static async Task<RespContent> GetVehiclesByPersonaId(string personaId)
    {
        var sw = new Stopwatch();
        sw.Start();

        var respContent = new RespContent();

        try
        {
            headers["X-GatewaySession"] = Globals.SessionId;
            respContent.IsSuccess = false;

            var reqBody = new
            {
                jsonrpc = "2.0",
                method = "Progression.getVehiclesByPersonaId",
                @params = new
                {
                    game = "tunguska",
                    personaId = personaId
                },
                id = Guid.NewGuid()
            };

            var request = new RestRequest()
                .AddHeaders(headers)
                .AddJsonBody(reqBody);

            var response = await client.ExecutePostAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                respContent.IsSuccess = true;
                respContent.Message = response.Content;
            }
            else
            {
                var respError = JsonUtil.JsonDese<RespError>(response.Content);

                respContent.Message = $"{respError.error.code} {respError.error.message}";
            }
        }
        catch (Exception ex)
        {
            respContent.Message = ex.Message;
        }

        sw.Stop();
        respContent.ExecTime = sw.Elapsed.TotalSeconds;

        return respContent;
    }
}
