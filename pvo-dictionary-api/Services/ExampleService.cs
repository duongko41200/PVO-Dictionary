using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pvo_dictionary_api.Common;
using pvo_dictionary_api.Models;
using pvo_dictionary_api.Repositories;
using pvo_dictionary_api.Request;
using System;
using System.Collections.Generic;

namespace pvo_dictionary_api.Services
{
    public class ExampleService
    {
        private readonly ToneRepository _toneRepository;
        private readonly ModeRepository _modeRepository;
        private readonly RegisterRepository _registerRepository;
        private readonly NuanceRepository _nuanceRepository;
        private readonly DialectRepository _dialectRepository;
        private readonly ExampleLinkRepository _exampleLinkRepository;
        private readonly ExampleRepository _exampleRepository;
        private readonly ConceptRepository _conceptRepository;
        private readonly ExampleRelationshipRepository _exampleRelationshipRepository;
        private readonly ApiOption _apiOption;
        private readonly IMapper _mapper;

        public ExampleService(ApiOption apiOption, AppDbContext databaseContext, IMapper mapper)
        {
            _toneRepository = new ToneRepository(apiOption, databaseContext, mapper);
            _modeRepository = new ModeRepository(apiOption, databaseContext, mapper);
            _registerRepository = new RegisterRepository(apiOption, databaseContext, mapper);
            _nuanceRepository = new NuanceRepository(apiOption, databaseContext, mapper);
            _dialectRepository = new DialectRepository(apiOption, databaseContext, mapper);
            _exampleLinkRepository = new ExampleLinkRepository(apiOption, databaseContext, mapper);
            _exampleRepository = new ExampleRepository(apiOption, databaseContext, mapper);
            _conceptRepository = new ConceptRepository(apiOption, databaseContext, mapper);
            _exampleRelationshipRepository = new ExampleRelationshipRepository(apiOption, databaseContext, mapper);
            _apiOption = apiOption;
            _mapper = mapper;
        }

        /// <summary>
        /// AddExample
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public AddExampleResult AddExample(int userId, AddExampleRequest request)
        {
            try
            {
                // Kiểm tra trường hợp không hợp lệ: Example trống nội dung
                if (string.IsNullOrEmpty(request.Detail))
                {
    
                    throw new ValidateError(9000, "Invalid parameters: Tham số để trống");
                }

                // Kiểm tra trường hợp không hợp lệ: Example có nội dung nhưng chưa có cụm từ nào được highlighted
                if (string.IsNullOrEmpty(request.DetailHtml))
                {
      
                    throw new ValidateError(9000, "Invalid parameters: Example content is missing highlighted phrases");
                }

                // Kiểm tra trường hợp không hợp lệ: Nội dung example vượt quá 2000 từ
                if (request.Detail.Length > 2000)
                {
                    throw new ValidateError(9000, "Invalid parameters: Example content exceeds 2000 characters");
                }

                foreach (var item in request.ListExampleRelationship)
                {
                    var concept = _conceptRepository.GetById(item.ConceptId);
                    var exampleLink = _exampleLinkRepository.GetById(item.ExampleLinkId);
             ;
                    if (concept != null && exampleLink != null)
                    {
                        

                        var example = new Example()
                        {
                            dictionary_id = concept.dictionary_id,
                            detail = request.Detail,
                            detail_html = request.DetailHtml,
                            note = request.Note,
                            tone_id = request.ToneId,
                            register_id = request.RegisterId,
                            dialect_id = request.DialectId,
                            mode_id = request.ModeId,
                            nuance_id = request.NuanceId,
                        };

                        _exampleRepository.Create(example);
                        _exampleRepository.SaveChange();

                        var exampleRelationship = new ExampleRelationship()
                        {
                            dictionary_id = concept.dictionary_id,
                            concept_id = item.ConceptId,
                            example_link_id = item.ExampleLinkId,
                            example_id = example.example_id,
                        };
                        _exampleRelationshipRepository.Create(exampleRelationship);
                        _exampleRelationshipRepository.SaveChange();

                        return new AddExampleResult
                        {
                            AddedExample = example,
                            AddedExampleRelationship = exampleRelationship
                        };
                    }
                }

                // If no example was added, you can return null or throw an exception
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        /// <summary>
        /// search example
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="toneId"></param>
        /// <param name="modeId"></param>
        /// <param name="registerId"></param>
        /// <param name="nuanceId"></param>
        /// <param name="dialectId"></param>
        /// <returns></returns>
        public object SearchExample(string? keyword, int? toneId, int? modeId, int? registerId, int? nuanceId, int? dialectId)
        {
            try
            {
                var query = _exampleRepository.GetAll();

                if(!string.IsNullOrEmpty(keyword) )
                {
                    query = query.Where(row => row.detail.ToLower().Contains(keyword.ToLower()));
                }
                if(toneId != null)
                {
                    query = query.Where(row => row.tone_id == toneId);
                }
                if(modeId != null)
                {
                    query = query.Where(row => row.mode_id == modeId);
                }
                if(registerId != null)
                {
                    query = query.Where(row => row.register_id == registerId);
                }
                if(nuanceId != null)
                {
                    query = query.Where(row => row.nuance_id == nuanceId);
                }
                if(dialectId != null)
                {
                    query = query.Where(row => row.dialect_id == dialectId);
                }

                return query.ToList();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// get example
        /// </summary>
        /// <param name="exampleId"></param>
        /// <returns></returns>
        public object GetExample(int exampleId)
        {
            try
            {
                var example = _exampleRepository.GetById(exampleId);
                var exampleRelationships = _exampleRelationshipRepository.GetByCondition(er => er.example_id == exampleId).ToList();


                var data = new
                {
                    ExampleId = example.example_id,
                    Detail = example.detail,
                    DetailHtml = example.detail_html,
                    Note = example.note,
                    Tone = example.tone_id,
                    Mode = example.mode_id,
                    Register = example.register_id,
                    Nuance = example.nuance_id,
                    Dialect = example.dialect_id,
                    ListExampleRelationship = exampleRelationships.Select(er => new
                    {
                        ExampleRelationshipId = er.example_relationship_id,
                        ConceptId = er.concept_id,
                       ExampleLinkId = er.example_link_id
                    }).ToList()
                 
                };

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public object GetLinkedExamplesByRelationshipType(int conceptId, int exampleLinkId)
        {
            try
            {
                var exampleRelationships = _exampleRelationshipRepository.GetByCondition(relationship => relationship.concept_id == conceptId && relationship.example_link_id == exampleLinkId)
                    .ToList();

                var exampleIds = exampleRelationships.Select(relationship => relationship.example_id).ToList();

                var examples = _exampleRepository.GetByCondition(example => exampleIds.Contains(example.example_id))
                    .Select(example => new
                    {
                        ExampleId = example.example_id,
                        Detail = example.detail,
                        DetailHtml = example.detail_html,
       
                    })
                    .ToList();

                return examples;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// update example
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public object UpdateExample(UpdateExampleRequest request)
        {
            try
            {
                var example = _exampleRepository.GetById(request.ExampleId);
                if(example == null)
                {
                    throw new ValidateError(2000, "Example doesn’t exist");
                }
               
                example.detail = request.Detail;
                example.detail_html = request.DetailHtml;
                example.note = request.Note;
                example.tone_id = request.ToneId;
                example.register_id = request.RegisterId;
                example.dialect_id = request.DialectId;
                example.mode_id = request.ModeId;
                example.nuance_id = request.NuanceId;

                _exampleRepository.UpdateByEntity(example);
                _exampleRepository.SaveChange();

                return example;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// delete example
        /// </summary>
        /// <param name="exampleId"></param>
        /// <returns></returns>
        public object DeleteExample(int exampleId)
        {
            try
            {
                var example = _exampleRepository.GetById(exampleId);
                if (example == null)
                {
                    throw new ValidateError(2000, "Example doesn’t exist");
                }

                _exampleRepository.DeleteByEntity(example);
                _exampleRepository.SaveChange();

                // @todo
                // delete relationship

                return example;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
      



    }
}
