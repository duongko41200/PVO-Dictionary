using AutoMapper;
using Microsoft.AspNetCore.Mvc.RazorPages;
using pvo_dictionary_api.Common;
using pvo_dictionary_api.Models;
using pvo_dictionary_api.Repositories;
using pvo_dictionary_api.Request;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;

namespace pvo_dictionary_api.Services
{
    public class DictionaryService
    {
        private readonly UserRepository _userRepository;
        private readonly DictionaryRepository _dictionaryRepository;
        private readonly ConceptRepository _conceptRepository;
        private readonly ExampleRepository _exampleRepository;
        private readonly ApiOption _apiOption;
        private readonly IMapper _mapper;

        public DictionaryService(ApiOption apiOption, AppDbContext databaseContext, IMapper mapper)
        {
            _userRepository = new UserRepository(apiOption, databaseContext, mapper);
            _dictionaryRepository = new DictionaryRepository(apiOption, databaseContext, mapper);
            _conceptRepository = new ConceptRepository(apiOption, databaseContext, mapper);
            _exampleRepository = new ExampleRepository(apiOption, databaseContext, mapper);
            _apiOption = apiOption;
            _mapper = mapper;
        }

        /// <summary>
        /// Get dictionary list by user
        /// </summary>
        /// <returns></returns>
        public object GetListDictionary(int userId)
        {
            try
            {
                var dictionariesList = _dictionaryRepository.GetAll().Where(row => row.user_id == userId).ToList();
                return dictionariesList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// access to dictionary
        /// </summary>
        /// <param name="dictionaryId"></param>
        /// <returns></returns>
        public object LoadDictionary(int userId, int dictionaryId)
        {
            try
            {
                var dictionary = _dictionaryRepository.GetById(dictionaryId);
                if(dictionary == null)
                {
                    throw new ValidateError(2000, "Dictionary doesn’t exist"); 
                }
                return dictionary;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Add Dictionary
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="addDictionaryRequest"></param>
        /// <returns></returns>
        public object AddDictionary(int userId, AddDictionaryRequest addDictionaryRequest)
        {
            try
            {
                
                if (addDictionaryRequest.CloneDictionaryId != 0)
                {
                    var dictionaryClone = _dictionaryRepository.GetById(addDictionaryRequest.CloneDictionaryId);
                    if (dictionaryClone == null || dictionaryClone.user_id != userId)
                    {
                        throw new ValidateError(2000, "Dictionary doesn’t exist");
                    }
                }

                var dictionaryByName = _dictionaryRepository.GetAll().Where(row => row.dictionary_name == addDictionaryRequest.DictionaryName && row.user_id == userId);
                if (dictionaryByName.Count() > 0)
                {
                    return null;
                }
                var existingDictionary = _dictionaryRepository.GetAll()
           .FirstOrDefault(row => row.dictionary_name == addDictionaryRequest.DictionaryName && row.user_id == userId);

                if (existingDictionary != null)
                {
                    throw new ValidateError(2001, "Dictionary name already in use: Tên từ điển bị trùng");
                }

                var newDictionary = new Dictionary()
                {
                    user_id = userId,
                    dictionary_name = addDictionaryRequest.DictionaryName,
                };

                // Clone case
                //if(addDictionaryRequest.CloneDictionaryId != null)
                //{
                //    newDictionary.
                //}

                _dictionaryRepository.Create(newDictionary);
                _dictionaryRepository.SaveChange();

                if(addDictionaryRequest.CloneDictionaryId != 0)
                {
                    var conceptList = _conceptRepository.GetByCondition(row => row.dictionary_id == addDictionaryRequest.CloneDictionaryId).ToList();
                    foreach (var concept in conceptList)
                    {
                        var newConcept = concept;
                        newConcept.concept_id = 0;
                        newConcept.dictionary_id = newDictionary.dictionary_id;
                        newConcept.created_date = DateTime.Now;
                        _conceptRepository.Create(newConcept);
                    }
                    _conceptRepository.SaveChange();
                }
                
                return newDictionary;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// update dictionary
        /// </summary>
        /// <param name="updateDictionaryRequest"></param>
        /// <returns></returns>
        public object UpdateDictionary(int userId, UpdateDictionaryRequest updateDictionaryRequest)
        {
            try
            {
                var dictionary = _dictionaryRepository.GetById(updateDictionaryRequest.DictionaryId);
                if(dictionary == null || dictionary.user_id != userId)
                {
                    throw new ValidateError(2000, "Dictionary doesn’t exist");
                }

                var dictionaryListByName = _dictionaryRepository.GetByCondition(row => row.dictionary_name == updateDictionaryRequest.DictionaryName);
                if (dictionaryListByName.Count() > 0)
                {
                    throw new ValidateError(2001, "Dictionary name already in use");
                }
                dictionary.dictionary_name = updateDictionaryRequest.DictionaryName.Trim();
                dictionary.updated_date = DateTime.Now;

                _dictionaryRepository.UpdateByEntity(dictionary); 
                _dictionaryRepository.SaveChange();
                return dictionary;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateDictionaryRequest"></param>
        /// <returns></returns>
        public object DeleteDictionary(int dictionaryId)
        {
            try
            {
                var dictionary = _dictionaryRepository.GetById(dictionaryId);
                if (dictionary == null)
                {
                    throw new ValidateError(2000, "Dictionary doesn’t exist");
                }
               
                _dictionaryRepository.DeleteByEntity(dictionary);
                _dictionaryRepository.SaveChange();
                return dictionary;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public object TranferDictionary(TranferDictionaryRequest tranferDictionaryRequest)
        {
            try
            {
                var sourceDictionary = _dictionaryRepository.GetById(tranferDictionaryRequest.SourceDictionaryId);
                if (sourceDictionary == null)
                {
                    throw new ValidateError(2003, "Source dictionary is empty");
                }
                // Check if the source dictionary has any examples
                
             
                var destDictionary = _dictionaryRepository.GetById(tranferDictionaryRequest.DestDictionaryId);
                if (destDictionary == null)
                {
                    throw new Exception("Dest dictionary is empty");
                }

                var conceptListBySourceDictionaryId = _conceptRepository.GetByCondition(row => row.dictionary_id == tranferDictionaryRequest.SourceDictionaryId).ToList();
                foreach (var concept in conceptListBySourceDictionaryId)
                {
                    concept.dictionary_id = tranferDictionaryRequest.DestDictionaryId;
                    _conceptRepository.UpdateByEntity(concept); // Update the concept instead of creating a new one
                    _conceptRepository.SaveChange();
                }

                var exampleListBySourceDictionaryId = _exampleRepository.GetByCondition(row => row.dictionary_id == tranferDictionaryRequest.SourceDictionaryId).ToList();
                if (exampleListBySourceDictionaryId.Count == 0)
                {
                    throw new ValidateError(2003, "Source dictionary is empty");
                }
                foreach (var example in exampleListBySourceDictionaryId)
                {
                    example.dictionary_id = tranferDictionaryRequest.DestDictionaryId;
                    _exampleRepository.UpdateByEntity(example); // Update the example instead of creating a new one
                    _exampleRepository.SaveChange();
                }

                return new { Concept = conceptListBySourceDictionaryId, Example = exampleListBySourceDictionaryId };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public object GetNumberRecord(int dictionaryId)
        {
            try
            {
                var dictionary = _dictionaryRepository.GetById(dictionaryId);
                if (dictionary == null)
                {
                    throw new Exception("Dictionary is empty");
                }

                var numberConcept = _conceptRepository.GetByCondition(row => row.dictionary_id == dictionaryId).Count();
                var numberExample = _exampleRepository.GetByCondition(row => row.dictionary_id == dictionaryId).Count();

                return new
                {
                    numberConcept = numberConcept,
                    numberExample = numberExample
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
      
    }
}
