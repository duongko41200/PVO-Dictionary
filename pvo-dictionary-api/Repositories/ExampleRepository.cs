﻿using AutoMapper;
using pvo_dictionary_api.Common;
using pvo_dictionary_api.Models;
using pvo_dictionary_api.Request;
using pvo_dictionary_api.Respositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace pvo_dictionary_api.Repositories
{
    public class ExampleRepository : BaseRespository<Example>
    {
        private readonly IMapper _mapper;
        public ExampleRepository(ApiOption apiConfig, AppDbContext databaseContext, IMapper mapper) : base(apiConfig, databaseContext)
        {
            _mapper = mapper;
        }
        
    }
}
