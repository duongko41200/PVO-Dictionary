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
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Microsoft.Extensions.Options;

namespace pvo_dictionary_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TemplateController : BaseApiController<TemplateController>
    {
        private readonly ConceptService _conceptService;
        private readonly DictionaryService _dictionaryService;

        private readonly AppDbContext _context;
        public TemplateController(AppDbContext databaseContext, IMapper mapper, ApiOption apiConfig)
        {
            _conceptService = new ConceptService(apiConfig, databaseContext, mapper);
            _dictionaryService = new DictionaryService(apiConfig, databaseContext, mapper);
            _context = databaseContext;
        }


        /// <summary>
        /// Get dictionary list by user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("download")]
        public async Task<IActionResult> download()
        {
            try
            {
                //return download file
                var filepath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "default_template_protect.xlsx");

                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(filepath, out var contenttype))
                {
                    contenttype = "application/octet-stream";
                }

                var bytes = await System.IO.File.ReadAllBytesAsync(filepath);
                return File(bytes, contenttype, Path.GetFileName(filepath));
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        [HttpGet]
        [Route("export")]
        public IActionResult Export(int dictionaryId)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the LicenseContext property

                var dictionaryData = _context.dictionaries
                        .Where(d => d.dictionary_id == dictionaryId && d.user_id == UserId)
                        .ToList();

                var conceptData = _context.concepts
                    .Where(c => c.dictionary_id == dictionaryId)
                    .ToList();

                var exampleData = _context.examples
                    .Where(e => e.dictionary_id == dictionaryId)
                    .ToList();
                var conceptRelationshipData = _context.concept_relationships
                   .Where(e => e.dictionary_id == dictionaryId)
                   .ToList();
                var exampleRelationshipData = _context.example_relationships
                   .Where(e => e.dictionary_id == dictionaryId)
                   .ToList();

                using (var package = new ExcelPackage())
                {
                    var dictionaryWorksheet = package.Workbook.Worksheets.Add("Dictionary Data");
                    var conceptWorksheet = package.Workbook.Worksheets.Add("Concept Data");
                    var exampleWorksheet = package.Workbook.Worksheets.Add("Example Data");
                    var conceptRelationshipWorksheet = package.Workbook.Worksheets.Add("Concept Relationship Data");
                    var exampleRelationshipWorksheet = package.Workbook.Worksheets.Add("Example Relationship Data");
                    // Set header text for Dictionary worksheet
                    dictionaryWorksheet.Cells[1, 1].Value = "dictionary_id";
                    dictionaryWorksheet.Cells[1, 2].Value = "dictionary_name";
                    dictionaryWorksheet.Cells[1, 3].Value = "last_view_at";
                    dictionaryWorksheet.Cells[1, 4].Value = "created_date";
                    dictionaryWorksheet.Cells[1, 5].Value = "updated_date";

                    // Set header text for Concept worksheet
                    conceptWorksheet.Cells[1, 1].Value = "title";
                    conceptWorksheet.Cells[1, 2].Value = "description";
                    conceptWorksheet.Cells[1, 3].Value = "created_date";
                    conceptWorksheet.Cells[1, 4].Value = "updated_date";
                    // Set header text for Example worksheet
                    exampleWorksheet.Cells[1, 1].Value = "detail";
                    exampleWorksheet.Cells[1, 2].Value = "detail_html";
                    exampleWorksheet.Cells[1, 3].Value = "note";
                    exampleWorksheet.Cells[1, 4].Value = "tone_id";
                    exampleWorksheet.Cells[1, 5].Value = "register_id";
                    exampleWorksheet.Cells[1, 6].Value = "dialect_id";
                    exampleWorksheet.Cells[1, 7].Value = "mode_id";
                    exampleWorksheet.Cells[1, 8].Value = "nuance_id";
                    exampleWorksheet.Cells[1, 9].Value = "created_date";
                    exampleWorksheet.Cells[1, 10].Value = "updated_date";
                    //
                    conceptRelationshipWorksheet.Cells[1, 1].Value = "concept_id";
                    conceptRelationshipWorksheet.Cells[1, 2].Value = "parent_id";
                    conceptRelationshipWorksheet.Cells[1, 3].Value = "concept_link_id";
                    conceptRelationshipWorksheet.Cells[1, 4].Value = "created_date";
                    conceptRelationshipWorksheet.Cells[1, 5].Value = "updated_date";
                    //
                    exampleRelationshipWorksheet.Cells[1, 1].Value = "concept_id";
                    exampleRelationshipWorksheet.Cells[1, 2].Value = "example_id";
                    exampleRelationshipWorksheet.Cells[1, 3].Value = "example_link_id";
                    exampleRelationshipWorksheet.Cells[1, 4].Value = "created_date";
                    exampleRelationshipWorksheet.Cells[1, 5].Value = "updated_date";
                    // Set all header cells to bold
                    var headerRange = dictionaryWorksheet.Cells[1, 1, 1, 5];
                    headerRange.Style.Font.Bold = true;

                    headerRange = conceptWorksheet.Cells[1, 1, 1, 4];
                    headerRange.Style.Font.Bold = true;

                    headerRange = exampleWorksheet.Cells[1, 1, 1, 10];
                    headerRange.Style.Font.Bold = true;
                    headerRange = conceptRelationshipWorksheet.Cells[1, 1, 1, 5];
                    headerRange.Style.Font.Bold = true;

                    headerRange = exampleRelationshipWorksheet.Cells[1, 1, 1, 5];
                    headerRange.Style.Font.Bold = true;

                    int rowIndex = 2;
                    foreach (var item in dictionaryData)
                    {
                        dictionaryWorksheet.Cells[rowIndex, 1].Value = item.dictionary_id;
                        dictionaryWorksheet.Cells[rowIndex, 2].Value = item.dictionary_name;
                        dictionaryWorksheet.Cells[rowIndex, 3].Value = item.last_view_at;
                        dictionaryWorksheet.Cells[rowIndex, 4].Value = item.created_date.ToString("yyyy-MM-dd HH:mm:ss");
                        dictionaryWorksheet.Cells[rowIndex, 5].Value = item.updated_date.ToString("yyyy-MM-dd HH:mm:ss");

                        rowIndex++;
                    }

                    rowIndex = 2;
                    foreach (var item in conceptData)
                    {
                        conceptWorksheet.Cells[rowIndex, 1].Value = item.title;
                        conceptWorksheet.Cells[rowIndex, 2].Value = item.description;
                        conceptWorksheet.Cells[rowIndex, 3].Value = item.created_date.ToString("yyyy-MM-dd HH:mm:ss");
                        conceptWorksheet.Cells[rowIndex, 4].Value = item.updated_date.ToString("yyyy-MM-dd HH:mm:ss");
                        rowIndex++;
                    }

                    rowIndex = 2;
                    foreach (var item in exampleData)
                    {
                        exampleWorksheet.Cells[rowIndex, 1].Value = item.detail;
                        exampleWorksheet.Cells[rowIndex, 2].Value = item.detail_html;
                        exampleWorksheet.Cells[rowIndex, 3].Value = item.note;
                        exampleWorksheet.Cells[rowIndex, 4].Value = item.tone_id;
                        exampleWorksheet.Cells[rowIndex, 5].Value = item.register_id;
                        exampleWorksheet.Cells[rowIndex, 6].Value = item.dialect_id;
                        exampleWorksheet.Cells[rowIndex, 7].Value = item.mode_id;
                        exampleWorksheet.Cells[rowIndex, 8].Value = item.nuance_id;
                        exampleWorksheet.Cells[rowIndex, 9].Value = item.created_date.ToString("yyyy-MM-dd HH:mm:ss");
                        exampleWorksheet.Cells[rowIndex, 10].Value = item.updated_date.ToString("yyyy-MM-dd HH:mm:ss");
                        rowIndex++;
                    }
                    rowIndex = 2;
                    foreach (var item in conceptRelationshipData)
                    {
                        dictionaryWorksheet.Cells[rowIndex, 1].Value = item.concept_id;
                        dictionaryWorksheet.Cells[rowIndex, 2].Value = item.parent_id;
                        dictionaryWorksheet.Cells[rowIndex, 3].Value = item.concept_link_id;
                        dictionaryWorksheet.Cells[rowIndex, 4].Value = item.created_date.ToString("yyyy-MM-dd HH:mm:ss");
                        dictionaryWorksheet.Cells[rowIndex, 5].Value = item.updated_date.ToString("yyyy-MM-dd HH:mm:ss");

                        rowIndex++;
                    }
                    rowIndex = 2;
                    foreach (var item in exampleRelationshipData)
                    {
                        dictionaryWorksheet.Cells[rowIndex, 1].Value = item.concept_id;
                        dictionaryWorksheet.Cells[rowIndex, 2].Value = item.example_id;
                        dictionaryWorksheet.Cells[rowIndex, 3].Value = item.example_link_id;
                        dictionaryWorksheet.Cells[rowIndex, 4].Value = item.created_date.ToString("yyyy-MM-dd HH:mm:ss");
                        dictionaryWorksheet.Cells[rowIndex, 5].Value = item.updated_date.ToString("yyyy-MM-dd HH:mm:ss");

                        rowIndex++;
                    }
                    MemoryStream stream = new MemoryStream();
                    package.SaveAs(stream);

                    stream.Position = 0;
                    string fileName = "DictionaryExport.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost]
        [Route("import")]
        public IActionResult Import()
        {
            try
            {
                var file = Request.Form.Files[0];

                if (file == null || file.Length <= 0)
                {
                    return BadRequest("Invalid file upload: No file or empty file");
                }
                // Check the file format or extension to ensure it's a valid template file
                var validExtensions = new[] { ".xlsx", ".xls" };
                var fileExtension = Path.GetExtension(file.FileName);
                if (!validExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                {
                    throw new ValidateError(9001, "Invalid file upload: Tệp tải lên không hợp lệ.");
                }

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the LicenseContext property

                using (var package = new ExcelPackage(file.OpenReadStream()))
                {
                    var conceptWorksheet = package.Workbook.Worksheets[1]; // Get the Concept worksheet
                    var exampleWorksheet = package.Workbook.Worksheets[2]; // Get the Example worksheet
                                                                           // Import logic for concepts
                                                                           // Import logic for concepts
                    int conceptCount = 0;
                    int conceptRowCount = conceptWorksheet.Dimension.End.Row;
                    for (int row = 2; row <= conceptRowCount; row++)
                    {
                        // Check if the row has any text values
                        var conceptTitle = conceptWorksheet.Cells[row, 2].Value?.ToString();
                        var conceptDescription = conceptWorksheet.Cells[row, 3].Value?.ToString();

                        if (!string.IsNullOrEmpty(conceptTitle) || !string.IsNullOrEmpty(conceptDescription))
                        {
                            // Save the concept to the database
                            var concept = new Concept
                            {
                                title = conceptTitle,
                                description = conceptDescription,
                                dictionary_id = 11 // Assuming you have a variable `dictionaryId` that represents the current dictionary's ID
                            };

                            _context.concepts.Add(concept);
                            _context.SaveChanges();

                            conceptCount++;
                        }
                    }
                    // Import logic for examples
                    int exampleCount = 0;
                    int exampleRowCount = exampleWorksheet.Dimension.End.Row;
                    for (int row = 2; row <= exampleRowCount; row++)
                    {
                        // Check if the row has any text values
                        var exampleDetail = exampleWorksheet.Cells[row, 2].Value?.ToString();
                        var exampleTone = exampleWorksheet.Cells[row, 3].Value?.ToString();
                        var exampleMode = exampleWorksheet.Cells[row, 4].Value?.ToString();
                        var exampleRegister = exampleWorksheet.Cells[row, 5].Value?.ToString();
                        var exampleNuance = exampleWorksheet.Cells[row, 6].Value?.ToString();
                        var exampleDialect = exampleWorksheet.Cells[row, 7].Value?.ToString();
                        var exampleNote = exampleWorksheet.Cells[row, 8].Value?.ToString();

                        if (!string.IsNullOrEmpty(exampleDetail) || !string.IsNullOrEmpty(exampleTone) ||
                            !string.IsNullOrEmpty(exampleMode) || !string.IsNullOrEmpty(exampleRegister) ||
                            !string.IsNullOrEmpty(exampleNuance) || !string.IsNullOrEmpty(exampleDialect) ||
                            !string.IsNullOrEmpty(exampleNote))
                        {
                            // Save the example to the database
                            var example = new Example
                            {
                                detail = exampleDetail,
                                detail_html = exampleDetail,
                                note = exampleNote,
                                tone_id = int.Parse(exampleTone),
                                register_id = int.Parse(exampleRegister),
                                dialect_id = int.Parse(exampleDialect),
                                mode_id = int.Parse(exampleMode),
                                nuance_id = int.Parse(exampleNuance),
                                dictionary_id = 11 // Assuming you have a variable `dictionaryId` that represents the current dictionary's ID
                            };

                            _context.examples.Add(example);
                            _context.SaveChanges();


                            exampleCount++;
                        }
                    }

                    // Return the import results
                    var importResult = new
                    {
                        NumberConcepts = conceptCount,
                        NumberExamples = exampleCount
                    };

                    return Ok(importResult);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }
    }
}
