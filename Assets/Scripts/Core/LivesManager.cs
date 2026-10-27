using System;
using UnityEngine;

namespace Matatiempo.Core
{
    public static class LivesManager
    {
        public const string LivesKey = "matatiempo_lives";
        public const int DefaultLives = 3;
        public const int MaxLives = 9999;
        public const int MaxRegenLives = 3;

        public const string RegenTimestampKey = "matatiempo_lives_regen_ts";
        public const int RegenIntervalSeconds = 1800;

        // --- Cache en memoria para evitar polling continuo de PlayerPrefs ---
        private static bool _cacheValid;
        private static int _cachedLives;
        private static long _lastCalcTime;

        /// <summary>
        /// Invalida la caché para forzar recálculo en la próxima lectura.
        /// Llamar después de cualquier operación que modifique las vidas.
        /// </summary>
        private static void InvalidateCache()
        {
            _cacheValid = false;
        }

        /// <summary>
        /// Fuerza escritura a disco y devuelve las vidas actualizadas.
        /// Útil al pausar la app o al cambiar de escena críticas.
        /// </summary>
        public static void ForceSync()
        {
            PlayerPrefs.Save();
        }

        public static int GetLives()
        {
            // Si la caché es válida y las vidas están al máximo de regeneración,
            // no hay nada que recalcular.
            if (_cacheValid && _cachedLives >= MaxRegenLives)
            {
                return _cachedLives;
            }

            long now = NowSeconds();

            // Si la caché es válida y no ha transcurrido suficiente tiempo como
            // para regenerar al menos una vida, devolver el valor cacheado.
            // (MaxRegenLives <= 3, así que el intervalo de 1800s es el límite)
            if (_cacheValid && (now - _lastCalcTime) < RegenIntervalSeconds)
            {
                return _cachedLives;
            }

            int lives = Mathf.Max(0, Mathf.Min(MaxLives, PlayerPrefs.GetInt(LivesKey, DefaultLives)));

            if (lives >= MaxRegenLives)
            {
                ClearRegenTimestamp_NoSave();
                _cachedLives = lives;
                _lastCalcTime = now;
                _cacheValid = true;
                return lives;
            }

            long last = GetRegenTimestamp();

            if (last <= 0 || now < last)
            {
                // Inicializar timestamp solo si no existe. No hacer Save() aquí.
                if (last <= 0)
                {
                    SetRegenTimestamp_NoSave(now);
                }
                _cachedLives = lives;
                _lastCalcTime = now;
                _cacheValid = true;
                return lives;
            }

            long elapsed = now - last;
            if (elapsed < RegenIntervalSeconds)
            {
                _cachedLives = lives;
                _lastCalcTime = now;
                _cacheValid = true;
                return lives;
            }

            long grant = elapsed / RegenIntervalSeconds;
            int newLives = (int)Math.Min((long)MaxRegenLives, (long)lives + grant);

            if (newLives >= MaxRegenLives)
            {
                ClearRegenTimestamp_NoSave();
            }
            else
            {
                SetRegenTimestamp_NoSave(last + grant * RegenIntervalSeconds);
            }
            PlayerPrefs.SetInt(LivesKey, newLives);
            // No PlayerPrefs.Save() here — getter path must not flush to disk.
            // Values persist in-memory; Save() deferred to AddLives() or ForceSync().

            if (newLives > lives)
            {
                GameEvents.NotifyLivesChanged(newLives);
            }

            _cachedLives = newLives;
            _lastCalcTime = now;
            _cacheValid = true;
            return newLives;
        }

        public static bool HasLives()
        {
            return GetLives() > 0;
        }

        public static int AddLives(int amount)
        {
            int lives = Mathf.Max(0, Mathf.Min(MaxLives, GetLives() + amount));
            PlayerPrefs.SetInt(LivesKey, lives);
            if (lives >= MaxLives)
            {
                ClearRegenTimestamp_NoSave();
            }
            else
            {
                EnsureRegenTimestamp_NoSave();
            }
            // Flush a disco solo en operaciones explícitas de modificación
            PlayerPrefs.Save();
            InvalidateCache();
            return lives;
        }

        public static int GetSecondsUntilNextLife()
        {
            int lives = GetLives();
            if (lives >= MaxRegenLives)
            {
                return 0;
            }
            long last = GetRegenTimestamp();
            if (last <= 0)
            {
                return RegenIntervalSeconds;
            }
            long remain = RegenIntervalSeconds - (NowSeconds() - last);
            remain = Math.Clamp(remain, 0L, RegenIntervalSeconds);
            return (int)remain;
        }

        private static long NowSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private static long GetRegenTimestamp()
        {
            long value;
            if (!long.TryParse(PlayerPrefs.GetString(RegenTimestampKey, "0"), out value) || value < 0)
            {
                return 0;
            }
            return value;
        }

        /// <summary>
        /// Escribe el timestamp de regeneración SIN flush a disco.
        /// El save se delega a operaciones de escritura deliberadas.
        /// </summary>
        private static void SetRegenTimestamp_NoSave(long timestamp)
        {
            PlayerPrefs.SetString(RegenTimestampKey, timestamp.ToString());
        }

        private static void EnsureRegenTimestamp_NoSave()
        {
            if (GetRegenTimestamp() <= 0)
            {
                SetRegenTimestamp_NoSave(NowSeconds());
            }
        }

        private static void ClearRegenTimestamp_NoSave()
        {
            if (PlayerPrefs.HasKey(RegenTimestampKey) && GetRegenTimestamp() > 0)
            {
                SetRegenTimestamp_NoSave(0);
            }
        }
    }
}