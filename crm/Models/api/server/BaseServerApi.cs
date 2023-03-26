﻿using crm.Models.api.server.serialization;
using crm.Models.api.server.valuesconverter;
using crm.Models.creatives;
using crm.Models.location;
using crm.Models.user;
using crm.ViewModels.tabs.home.menu.items;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using geo = crm.Models.geoservice;

namespace crm.Models.api.server
{
    public abstract class BaseServerApi : IServerApi
    {

        #region const
        Dictionary<string, string> serverErrorsDictionary = new Dictionary<string, string>()
        {
            {"auth-0001", "Токен не найден"},
            {"auth-0002", "Неопознанный токен"},
            {"auth-0003", "Доступ запрещен"},
            {"auth-0004", "Не задан адрес электронной почты"},
            {"auth-0005", "Не указан пароль"},
            {"auth-0006", "Не задано имя пользователя"},
            {"auth-0007", "Не указана дата рождения"},
            {"auth-0008", "Не указан номер телефона"},
            {"auth-0009", "Не указан адрес телеграм"},
            {"auth-0010", "Не указан номер кошелька"},
            {"auth-0011", "Не указаны устройства"},
            {"auth-0012", "Пароль не должен быть короче 12 символов"},
            {"auth-0013", "Пароль должен содержать цифры, строчные и заглавные буквы"},
            {"auth-0014", "Электронный адрес должен принадлежать protonmail.com"},
            {"auth-0015", "Не задан ID"},
            {"auth-0016", "ID должен быть целым числом"},
            {"auth-0017", "Неправильный электронный адрес"},
            {"auth-0018", "Неверный формат электронной почты"},
            {"auth-0019", "Не задана буква пользователя"},
            {"auth-0050", "Не удалось хэшировать пароль"},
            {"auth-0051", "Пользователь уже существует"},
            {"auth-0052", "Не удалось создать пользователя"},
            {"auth-0053", "Не удалось добавить права пользователю"},
            {"auth-0054", "Не удалось получить список пользователей"},
            {"auth-0055", "Нет зарегистрированных пользователей"},
            {"auth-0056", "Пользователь на найден"},
            {"auth-0057", "Не удалось получить ID пользователя"},
            {"auth-0058", "Пользователь с таким адресом не зарегистрирован"},
            {"auth-0059", "Не удалось создать токен доступа"},
            {"auth-0060", "Неверный пароль"},
            {"auth-0061", "Не удалось получить ID из токена"},
            {"auth-0062", "Не удалось получить пользователя по ID"}

        };

        #endregion

        #region vars
        protected string url;
        IConverter converter;
        #endregion

        public BaseServerApi(string url)
        {
            this.url = url;
            converter = new Converter();
        }

        #region helpers
        string getErrMsg(List<ServerError> errs)
        {
            string res = "";
            foreach (var item in errs)
            {
                if (serverErrorsDictionary.ContainsKey(item.code))
                    res += $"{serverErrorsDictionary[item.code]}\n";
                else
                    res += $"{item.code} {item.message}\n";
            }
            return res;
        }
        #endregion

        #region public
        public virtual async Task<bool> ValidateRegToken(string token)
        {
            bool res = false;
            try
            {
                await Task.Run(() => {
                    var client = new RestClient($"{url}/v1/auth/validateRegToken");
                    var request = new RestRequest(Method.POST);
                    request.AddHeader($"Authorization", $"Bearer {token}");
                    IRestResponse response = client.Execute(request);
                    //if (response.StatusCode != HttpStatusCode.OK)
                    //    throw new ServerResponseException(response.StatusCode);
                    JObject json = JObject.Parse(response.Content);
                    res = json["success"].ToObject<bool>();
                    if (!res)
                    {
                        string e = json["errors"].ToString();
                        List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                        throw new ServerException($"{getErrMsg(errors)}");
                    }

                });

            } catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
            return res;
        }

        public virtual async Task<bool> RegisterUser(string token, BaseUser user)
        {
            bool res = false;

            await Task.Run(() => {
                var client = new RestClient($"{url}/v1/users");
                var request = new RestRequest(Method.POST);
                request.AddHeader($"Authorization", $"Bearer {token}");
                dynamic p = new JObject();
                p.email = user.Email;
                p.password = user.Password;
                p.userfullname = user.FullName;
                p.birthday = converter.date(user.BirthDate, Direction.user_server);
                p.phone = converter.phone(user.PhoneNumber, Direction.user_server);
                p.telegram = converter.telegram(user.Telegram, Direction.user_server);
                p.usdt_account = user.Wallet;
                p.firstname = user.FirstName;
                p.middlename = user.MiddleName;
                p.lastname = user.LastName;
                p.fullname = $"{user.LastName} {user.FirstName} {user.MiddleName}";

                request.AddParameter("application/json", p.ToString(), ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                JObject json = JObject.Parse(response.Content);
                if (!response.IsSuccessful)
                {
                    string e = json["errors"].ToString();
                    List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                    throw new ServerException($"{getErrMsg(errors)}");
                }
                res = json["success"].ToObject<bool>();
                if (res == null)
                    throw new NoDataReceivedException();
                if (!res)
                    throw new ApiException("Не удалось зарегистрировать пользователя");

            });

            return res;
        }

        public virtual async Task<BaseUser> Login(string login, string password)
        {
            User user = null;

            await Task.Run(async () => {

                var client = new RestClient($"{url}/v1/auth");
                var request = new RestRequest(Method.POST);
                dynamic p = new JObject();
                p.email = login;
                p.password = password;
                request.AddParameter("application/json", p.ToString(), ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                if (!response.IsSuccessful)
                {
                    string e = "Не удается войти в вашу учетную запись";
                    throw new ServerException($"{e}");
                }

                JObject json = JObject.Parse(response.Content);

                bool res = json["success"].ToObject<bool>();
                string token, id;


                if (res)
                {
                    token = json["data"]["token"].ToObject<string>();
                    id = json["data"]["userID"].ToObject<string>();
                } else
                {
                    string e = json["errors"].ToString();
                    List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                    throw new ServerException($"{getErrMsg(errors)}");
                }

                if (res)
                {
                    user = await GetUser(id, token);
                }

            });
            return user;

        }

        public virtual async Task<User> GetUser(string id, string token)
        {
            User user = new User();

            await Task.Run(() => {

                var client = new RestClient($"{url}/v1/users/{id}");
                var request = new RestRequest(Method.GET);
                request.AddHeader($"Authorization", $"Bearer {token}");
                var response = client.Execute(request);
                var json = JObject.Parse(response.Content);
                var res = json["success"].ToObject<bool>();
                if (res)
                {
                    JToken data = json["data"];
                    if (data != null)
                    {
                        user = JsonConvert.DeserializeObject<User>(data.ToString());
                    }

                } else
                {
                    string e = json["errors"].ToString();
                    List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                    throw new ServerException($"{getErrMsg(errors)}");
                }
            });

            user.Id = id;
            user.Token = token;

            return user;
        }

        public virtual async Task<(List<User>, int, int)> GetUsers(int page, int size, string token, string sortparameter, bool show_deleted = false)
        {
            List<User> users = new List<User>();
            int total_pages = 0;
            int total_users = 0;

            var client = new RestClient($"{url}/v1/users/");
            var request = new RestRequest(Method.GET);
            request.AddHeader($"Authorization", $"Bearer {token}");
            request.AddQueryParameter("page", page.ToString());
            request.AddQueryParameter("size", size.ToString());
            request.AddQueryParameter("deleted", show_deleted.ToString().ToLower());
            request.AddQueryParameter("sort_by", $"-enabled,{sortparameter}");
            var response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            var res = json["success"].ToObject<bool>();
            if (res)
            {
                JToken data = json["data"];
                if (data != null)
                {
                    users = JsonConvert.DeserializeObject<List<User>>(data.ToString());
                    total_pages = json["total_pages"].ToObject<int>();
                    total_users = json["total_users"].ToObject<int>();
                }
            } else
            {
                string e = json["errors"].ToString();
                List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                throw new ServerException($"{getErrMsg(errors)}");
            }

            return (users, total_pages, total_users);
        }

        public virtual async Task<string> GetNewUserToken(List<Role> roles, int office_id, string token)
        {
            string newtoken = string.Empty;

            await Task.Run(async () => {
                var client = new RestClient($"{url}/v1/admintools/getRegToken/");
                var request = new RestRequest(Method.POST);
                request.AddHeader($"Authorization", $"Bearer {token}");

                sroles r = new sroles(roles);
                r.office_id = office_id;
                string jroles = JsonConvert.SerializeObject(r);

                request.AddParameter("application/json", jroles, ParameterType.RequestBody);
                //request.AddParameter("office_id", office_id);
                var response = client.Execute(request);
                var json = JObject.Parse(response.Content);
                var res = json["success"].ToObject<bool>();
                if (res)
                {
                    JToken data = json["data"];
                    if (data != null)
                    {
                        newtoken = data["token"].ToString();
                    }

                } else
                {
                    string e = json["errors"].ToString();
                    List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                    throw new ServerException($"{getErrMsg(errors)}");
                }

            });

            return newtoken;
        }

        public virtual async Task<bool> UpdateUserInfo(string token, BaseUser user)
        {
            bool res = false;

            await Task.Run(() => {

                var client = new RestClient($"{url}/v1/users/{user.Id}");
                var request = new RestRequest(Method.PUT);
                request.AddHeader($"Authorization", $"Bearer {token}");

                user.BirthDate = converter.date(user.BirthDate, Direction.user_server);
                user.PhoneNumber = converter.phone(user.PhoneNumber, Direction.user_server);
                user.Telegram = converter.telegram(user.Telegram, Direction.user_server);

                string juser = JsonConvert.SerializeObject(user);

                request.AddParameter("application/json", juser, ParameterType.RequestBody);
                var response = client.Execute(request);
                var json = JObject.Parse(response.Content);
                res = json["success"].ToObject<bool>();
                if (!res)
                {
                    string e = json["errors"].ToString();
                    List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                    throw new ServerException($"{getErrMsg(errors)}");
                }
            });

            return res;
        }

        public virtual async Task<bool> UpdateUserComment(string token, BaseUser user)
        {
            bool res = false;
            var client = new RestClient($"{url}/v1/users/{user.Id}/description");
            var request = new RestRequest(Method.PUT);
            request.AddHeader($"Authorization", $"Bearer {token}");
            request.AddParameter("description", user.Description);
            var response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            res = json["success"].ToObject<bool>();
            if (!res)
            {
                string e = json["errors"].ToString();
                List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                throw new ServerException($"{getErrMsg(errors)}");
            }
            return res;
        }

        public virtual async Task<bool> UpdateUserPassword(string token, BaseUser user, string password)
        {
            bool res = false;
            var client = new RestClient($"{url}/v1/users/{user.Id}/password");
            var request = new RestRequest(Method.PUT);
            request.AddHeader($"Authorization", $"Bearer {token}");
            request.AddParameter("password", password);
            var response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            res = json["success"].ToObject<bool>();
            if (!res)
            {
                string e = json["errors"].ToString();
                List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                throw new ServerException($"{getErrMsg(errors)}");
            }
            return res;
        }
        public virtual async Task DeleteUser(string token, BaseUser user)
        {
            var client = new RestClient($"{url}/v1/users/{user.Id}");
            var request = new RestRequest(Method.DELETE);
            request.AddHeader($"Authorization", $"Bearer {token}");
            var response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            bool res = json["success"].ToObject<bool>();
            if (!res)
            {
                string e = json["errors"].ToString();
                List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                throw new ServerException($"{getErrMsg(errors)}");
            }
        }

        public virtual async Task<List<geo.GEO>> GetGeos(string token, string sortparameter)
        {
            List<geo.GEO> geos = new List<geo.GEO>();
            var client = new RestClient($"{url}/v1/geolocations/");
            var request = new RestRequest(Method.GET);
            request.AddHeader($"Authorization", $"Bearer {token}");
            request.AddQueryParameter("sort_by", sortparameter);
            var response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            var res = json["success"].ToObject<bool>();
            if (res)
            {
                JToken data = json["data"];
                if (data != null)
                {
                    geos = JsonConvert.DeserializeObject<List<geo.GEO>>(data.ToString());
                }
            } else
            {
                string e = json["errors"].ToString();
                List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                throw new ServerException($"{getErrMsg(errors)}");
            }
            return geos;
        }

        public virtual async Task<List<CreativeServerDirectory>> GetCreativeServerDirectories(string token)
        {
            List<CreativeServerDirectory> dirs = new();
            var client = new RestClient($"{url}/v1/creatives/directories");
            var request = new RestRequest(Method.GET);
            request.AddHeader($"Authorization", $"Bearer {token}");
            var response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            var res = json["success"].ToObject<bool>();
            if (res)
            {
                JToken data = json["data"];
                if (data != null)
                {
                    dirs = JsonConvert.DeserializeObject<List<CreativeServerDirectory>>(data.ToString());
                }
            } else
            {
                string e = json["errors"].ToString();
                List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                throw new ServerException($"{getErrMsg(errors)}");
            }
            return dirs;
        }

        class CreativeParameters
        {
            public string filename { get; set; }
            public string file_extension { get; set; }
            public int creo_directory_id { get; set; }
            public int office_id { get; set; }
            public bool is_private { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="filename"></param>
        /// <param name="geo"></param>
        /// <returns>(creative_name, filepath)</returns>
        /// <exception cref="ServerException"></exception>
        public virtual async Task<(int, string, string, string)> AddCreative(string token, string filename, string extension, CreativeServerDirectory dir, int office_id, bool is_private)
        {
            int creative_id = 0;
            string creative_name = "";
            string filepath = "";
            string file_uuid = "";

            var client = new RestClient($"{url}/v1/creatives");
            var request = new RestRequest(Method.POST);
            request.AddHeader($"Authorization", $"Bearer {token}");

            CreativeParameters cp = new()
            {
                filename = filename,
                file_extension = extension,
                creo_directory_id = dir.id,
                office_id = office_id,
                is_private = is_private
            };

            string scp = JsonConvert.SerializeObject(cp);
            request.AddParameter("application/json", scp, ParameterType.RequestBody);

            var response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            bool res = json["success"].ToObject<bool>();
            if (res)
            {
                JToken data = json["data"];
                if (data != null)
                {
                    creative_id = data["creative_id"].ToObject<int>();
                    creative_name = data["creative_name"].ToString();
                    filepath = data["filepath"].ToString();
                    file_uuid = data["file_uuid"].ToString();
                }

            } else
            {
                string e = json["errors"].ToString();
                List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                throw new ServerException($"{getErrMsg(errors)}");
            }

            return (creative_id, creative_name, filepath, file_uuid);
        }

        class StatusParameters
        {
            public bool uploaded { get; set; }
            public bool visibility { get; set; }
        }
        public virtual async Task SetCreativeStatus(string token, int id, bool isUploaded, bool isVisible)
        {
            var client = new RestClient($"{url}/v1/creatives/{id}");
            var request = new RestRequest(Method.PUT);
            request.AddHeader($"Authorization", $"Bearer {token}");

            StatusParameters sp = new();
            sp.visibility = isVisible;
            sp.uploaded = isUploaded;

            string ssp = JsonConvert.SerializeObject(sp);
            request.AddParameter("application/json", ssp, ParameterType.RequestBody);
            
            await Task.Run(() => {
                var response = client.Execute(request);
                var json = JObject.Parse(response.Content);
                bool res = json["success"].ToObject<bool>();
                if (res)
                {
                } else
                {
                    string e = json["errors"].ToString();
                    List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                    throw new ServerException($"{getErrMsg(errors)}");
                }
            });
        }

        public virtual async Task SetVisibility(string token, int id, bool isVisible)
        {
            var client = new RestClient($"{url}/v1/creatives/{id}");
            var request = new RestRequest(Method.PUT);
            request.AddHeader($"Authorization", $"Bearer {token}");

            StatusParameters sp = new();
            sp.visibility = isVisible;            

            string ssp = JsonConvert.SerializeObject(sp);
            request.AddParameter("application/json", ssp, ParameterType.RequestBody);

            await Task.Run(() => {
                var response = client.Execute(request);
                var json = JObject.Parse(response.Content);
                bool res = json["success"].ToObject<bool>();
                if (res)
                {
                }
                else
                {
                    string e = json["errors"].ToString();
                    List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                    throw new ServerException($"{getErrMsg(errors)}");
                }
            });
        }

        //public virtual async Task<(List<CreativeDTO>, int, int)> GetAvaliableCreatives(string token, int page, int size, CreativeServerDirectory dir, int filetype, bool? showinvisible)
        //{
        //    List<CreativeDTO> creatives = new();
        //    int total_pages = 0;
        //    int total_creatives = 0;

        //    var client = new RestClient($"{url}/v1/creatives/");
        //    var request = new RestRequest(Method.GET);
        //    request.AddHeader($"Authorization", $"Bearer {token}");
        //    request.AddQueryParameter("page", page.ToString());
        //    request.AddQueryParameter("size", size.ToString());
        //    request.AddQueryParameter("creo_directory_id", dir.id.ToString());
        //    request.AddQueryParameter("file_type_id", filetype.ToString());
        //    request.AddQueryParameter("sort_by", "+id");
        //    if (showinvisible != null)
        //        request.AddQueryParameter("visibility", $"{showinvisible}");
        //    var response = client.Execute(request);
        //    var json = JObject.Parse(response.Content);
        //    var res = json["success"].ToObject<bool>();
        //    if (res)
        //    {
        //        JToken data = json["data"];
        //        if (data != null)
        //        {
        //            creatives = JsonConvert.DeserializeObject<List<CreativeDTO>>(data.ToString());
        //            total_pages = json["total_pages"].ToObject<int>();
        //            total_creatives = json["total_users"].ToObject<int>();
        //        }
        //    } else
        //    {
        //        string e = json["errors"].ToString();
        //        List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
        //        throw new ServerException($"{getErrMsg(errors)}");
        //    }            

        //    return (creatives, total_pages, total_creatives);
        //}


        public virtual (List<CreativeDTO>, int, int) GetAvaliableCreatives(string token, int page, int size, CreativeServerDirectory dir, int filetype, bool? showinvisible)
        {
            List<CreativeDTO> creatives = new();
            int total_pages = 0;
            int total_creatives = 0;

            var client = new RestClient($"{url}/v1/creatives/");
            var request = new RestRequest(Method.GET);
            request.AddHeader($"Authorization", $"Bearer {token}");
            request.AddQueryParameter("page", page.ToString());
            request.AddQueryParameter("size", size.ToString());
            request.AddQueryParameter("creo_directory_id", dir.id.ToString());
            request.AddQueryParameter("file_type_id", filetype.ToString());
            request.AddQueryParameter("sort_by", "+id");
            if (showinvisible != null)
                request.AddQueryParameter("visibility", $"{showinvisible}");
            var response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            var res = json["success"].ToObject<bool>();
            if (res)
            {
                JToken data = json["data"];
                if (data != null)
                {
                    creatives = JsonConvert.DeserializeObject<List<CreativeDTO>>(data.ToString());
                    total_pages = json["total_pages"].ToObject<int>();
                    total_creatives = json["total_items"].ToObject<int>();
                }
            } else
            {
                string e = json["errors"].ToString();
                List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                throw new ServerException($"{getErrMsg(errors)}");
            }

            return (creatives, total_pages, total_creatives);
        }

        public virtual (List<CreativeDTO>, int, int) GetAvaliableCreatives(string token, int page, int size, CreativeServerDirectory dir, bool is_private, int office_id, string user_id, int filetype, bool? showinvisible)
        {
            List<CreativeDTO> creatives = new();
            int total_pages = 0;
            int total_creatives = 0;

            var client = new RestClient($"{url}/v1/creatives/");
            var request = new RestRequest(Method.GET);
            request.AddHeader($"Authorization", $"Bearer {token}");
            request.AddQueryParameter("page", page.ToString());
            request.AddQueryParameter("size", size.ToString());
            request.AddQueryParameter("creo_directory_id", dir.id.ToString());
            request.AddQueryParameter("is_private", is_private.ToString());
            if(office_id >= 0)
                request.AddQueryParameter("office_id", office_id.ToString());
            if((user_id != null) && (user_id.Length != 0))
                request.AddQueryParameter("created_by", user_id);
            request.AddQueryParameter("file_type_id", filetype.ToString());
            request.AddQueryParameter("sort_by", "+id");
            if (showinvisible != null)
                request.AddQueryParameter("visibility", $"{showinvisible}");
            var response = client.Execute(request);
            var json = JObject.Parse(response.Content);
            var res = json["success"].ToObject<bool>();
            if (res)
            {
                JToken data = json["data"];
                if (data != null)
                {
                    creatives = JsonConvert.DeserializeObject<List<CreativeDTO>>(data.ToString());
                    total_pages = json["total_pages"].ToObject<int>();
                    total_creatives = json["total_items"].ToObject<int>();
                }
            }
            else
            {
                string e = json["errors"].ToString();
                List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                throw new ServerException($"{getErrMsg(errors)}");
            }

            return (creatives, total_pages, total_creatives);
        }

        public class jdates
        {
            public string hire_date { get; set; }
            public string dismissal_date { get; set; }
        }
        public virtual async Task<bool> UpdateEmploymentDates(string token, BaseUser user)
        {
            bool res = false;
            var client = new RestClient($"{url}/v1/usersEmployment/{user.Id}");
            var request = new RestRequest(Method.PUT);
            request.AddHeader($"Authorization", $"Bearer {token}");

            var splt_h = user.HireDate?.Split(".");
            var hire = (splt_h != null) ? $"{splt_h[2]}-{splt_h[1]}-{splt_h[0]}" : null;

            var splt_d = user.DismissalDate?.Split(".");
            var diss = (splt_d != null) ? $"{splt_d[2]}-{splt_d[1]}-{splt_d[0]}" : null;

            jdates dates = new jdates()
            {
                //hire_date = user.HireDate,
                //dismissal_date = user.DismissalDate
                hire_date = hire,
                dismissal_date = diss

            };

            //dynamic p = new JObject();
            //p.hire_date = user.HireDate;
            //p.dismissal_date = (!string.IsNullOrEmpty(user.DismissalDate)) ? user.DismissalDate : null;

            //request.AddParameter("application/json", p.ToString(), ParameterType.RequestBody);

            string param = JsonConvert.SerializeObject(dates);
            request.AddParameter("application/json", param, ParameterType.RequestBody);

            await Task.Run(() => {
                var response = client.Execute(request);
                var json = JObject.Parse(response.Content);
                res = json["success"].ToObject<bool>();
                if (res)
                {
                }
                else
                {
                    string e = json["errors"].ToString();
                    List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                    throw new ServerException($"{getErrMsg(errors)}");
                }
            });
            return res;
        }

        public virtual async Task<List<LocationOfficeServer>> GetLocationOfficeServer(string token)
        {
            List<LocationOfficeServer> locationOffices = new();

            var client = new RestClient($"{url}/v1/offices/");
            var request = new RestRequest(Method.GET);
            request.AddHeader($"Authorization", $"Bearer {token}");
            request.AddQueryParameter("sort_by", "+id");
            await Task.Run(() =>
            {
                var response = client.Execute(request);
                var json = JObject.Parse(response.Content);
                var res = json["success"].ToObject<bool>();
                if (res)
                {
                    JToken data = json["data"];
                    if (data != null)
                    {
                        locationOffices = JsonConvert.DeserializeObject<List<LocationOfficeServer>>(data.ToString());
                    }
                    else
                    {
                        string e = json["errors"].ToString();
                        List<ServerError>? errors = JsonConvert.DeserializeObject<List<ServerError>>(e);
                        throw new ServerException($"{getErrMsg(errors)}");
                    }
                }
            });
            return locationOffices;
        }
        #endregion

    }

}
