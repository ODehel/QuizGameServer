using QuizGame.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuizGame.Infrastructure.Services;

/// <summary>
/// Service pour scanner et découvrir les buzzers sur le réseau Freebox
/// </summary>
public interface IBuzzerDiscoveryService
{
    /// <summary>
    /// Scanne le réseau et retourne les buzzers dont le nom commence par le préfixe spécifié
    /// </summary>
    Task<List<Buzzer>> DiscoverBuzzersAsync(string namePrefix = "Quiz");

    /// <summary>
    /// Scanne le réseau pour trouver tous les périphériques
    /// </summary>
    Task<List<Buzzer>> ScanNetworkAsync();
}
