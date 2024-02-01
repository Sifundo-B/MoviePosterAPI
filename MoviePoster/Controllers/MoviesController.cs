using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoviePoster.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

[ApiController]
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MoviesController> _logger; // Add logger
    private static List<string> _searchHistory = new List<string>();

    public MoviesController(IHttpClientFactory httpClientFactory, ILogger<MoviesController> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchMovies(string title)
    {
        try
        {
            var searchResults = await GetSearchResultsFromApi(title);

            _searchHistory.Insert(0, title);
            if (_searchHistory.Count > 5)
            {
                _searchHistory = _searchHistory.Take(5).ToList();
            }

            return Ok(new
            {
                SearchResults = searchResults,
                SearchHistory = _searchHistory
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in SearchMovies: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("details/{imdbId}")]
    public async Task<IActionResult> GetMovieDetails(string imdbId)
    {
        try
        {
            var movieDetails = await GetMovieDetailsFromApi(imdbId);

            return Ok(movieDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in GetMovieDetails: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }

    private async Task<List<OmdbMovie>> GetSearchResultsFromApi(string title)
    {
        string apiKey = "28ded0b7";
        string apiUrl = $"http://www.omdbapi.com/?apikey={apiKey}&s={title}";

        HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<OmdbSearchResult>();
            return result.Search;
        }
        else
        {
            _logger.LogError($"Error in GetSearchResultsFromApi: {response.ReasonPhrase}");
            return new List<OmdbMovie>(); // Return an empty list for error cases
        }
    }

    private async Task<OmdbMovieDetail> GetMovieDetailsFromApi(string imdbId)
    {
        string apiKey = "28ded0b7";
        string apiUrl = $"http://www.omdbapi.com/?apikey={apiKey}&i={imdbId}";

        HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<OmdbMovieDetail>();
            return result;
        }
        else
        {
            _logger.LogError($"Error in GetMovieDetailsFromApi: {response.ReasonPhrase}");
            return new OmdbMovieDetail(); // Return a default value for error cases
        }
    }
}