using AutoMapper;
using AutoMapper.Configuration;
using pvo_dictionary_api.Common;
using pvo_dictionary_api.Controllers;
using pvo_dictionary_api.Dto;
using pvo_dictionary_api.Request;
using pvo_dictionary_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Net.Http;
using pvo_dictionary_api.Models;

namespace pvo_dictionary_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : BaseApiController<UserController>
    {
        private readonly UserService _userService;
        public UserController(AppDbContext databaseContext, IMapper mapper, ApiOption apiConfig)
        {
            _userService = new UserService(apiConfig, databaseContext, mapper);
        }

        /// <summary>
        /// update password
        /// </summary>
        /// <param name="userRegisterRequest"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("update_password")]
        public MessageData UpdatePassword(UpdatePasswordRequest updatePasswordRequest)
        {
            try
            {
                var res = _userService.UpdatePassword(UserId, updatePasswordRequest);
                if (res == null)
                {
                    return new MessageData { Data = res, Status = -1, ErrorCode = 1000, Message = "Incorrect email or password: Mật khẩu cũ cung cấp không đúng" };
                }
                return new MessageData { Data = res, Status = 1 };
            }
            catch (Exception ex)
            {
                return new MessageData() { Code = "Fail", Message = ex.Message, Status = 2, ErrorCode = 1000 };
            }
        }

        /// <summary>
        /// update user info
        /// </summary>
        /// <param name="updateUserInforRequest"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route("update_user_info")]
       
        public MessageData UpdateUserInfor([FromForm] UpdateUserInforRequest updateUserInforRequest)
        {
            try
            {
                var res = _userService.UpdateUserInfor(UserId, updateUserInforRequest);
                return new MessageData { Data = res, Status = 1 };
            }
            catch (Exception ex)
            {
                return new MessageData() { Code = "Fail", Message = ex.Message, Status = 2};
            }
        }
    }
}
