using ATS.MVVM.Collections;
using ATS.MVVM.Core;
using Dissertation.Modeling.Helpers;
using Dissertation.Modeling.Model.Basics;
using System;
using System.Collections.Generic;

namespace Dissertation.Modeling.Model.OrbitalSystem
{
    /// <summary>
    /// Орбитальная система, которая может состоять из нескольких ярусов (минимум 1)
    /// </summary>
    public class OrbitalSystem : VirtualBindableBase
    {
        /// <summary>
        /// Конструктор для инициализации системы как минимум с 1-им ярусом
        /// </summary>
        /// <param name="tier"></param>
        public OrbitalSystem(params Tier[] tiers)
        {
            if (tiers == null && tiers.Length < 1)
                throw new ArgumentNullException(nameof(tiers));

            _Tiers = new List<Tier>(tiers.Length);
            for (int i = 0; i < tiers.Length; i++)
                AddTier(tiers[i], true);
            CalculateOrbitalSystemParameters();
        }
        /// <summary>
        /// Поле для хранения ярусов сисетмы
        /// </summary>
        List<Tier> _Tiers;
        /// <summary>
        /// Ярусы системы (доступ только для чтения)
        /// Для добавления нового яруса необходимо использовать метод AddTier
        /// </summary>
        public IReadOnlyList<Tier> Tiers => _Tiers;
        /// <summary>
        /// Аналзируемая протяженность долготы
        /// </summary>
        public Angle AnalysingLongitudeDistance { get; private set; }
        /// <summary>
        /// Период повторяемости орбитальной системы
        /// </summary>
        public double EraTier { get; private set; }
        /// <summary>
        /// Добавить новый ярус в систему
        /// При добавлении будут пересчитаны значения периода повторяемости орбитальной системы, 
        /// а также анализируемая протяженность долготы
        /// </summary>
        /// <param name="tier"></param>
        /// <param name="deferredCalculation"></param>
        public void AddTier(Tier tier, bool deferredCalculation = false)
        {
            if (tier == null)
                throw new ArgumentNullException(nameof(tier));

            _Tiers.Add(tier);
            if (!deferredCalculation)
                CalculateOrbitalSystemParameters();
        }
        /// <summary>
        /// Расчет параметров орбитальной системы
        /// </summary>
        void CalculateOrbitalSystemParameters()
        {
            EraTier = CalculateOrbitalSystemEraTier();
            AnalysingLongitudeDistance = CalculateOrbitalSystemAnalysisLongitudeInterval();
        }
        /// <summary>
        /// Протяженность долготы, на которой анализируются участки инвариантности 
        /// Для одноярусной системы равно межузловому расстоянию 
        /// </summary>
        Angle CalculateOrbitalSystemAnalysisLongitudeInterval()
        {
            int m = 1;
            m = _Tiers[0].Orbit.NCoil;
            for (int i = 1; i < Tiers.Count; i++)
            {
                int k = _Tiers[i].Orbit.NCoil;
                m = MathEx.NOD(m, k);
            }
            return new Angle(Math.PI * 2 / m, true);
        }
        /// <summary>
        /// Период повторяемости обритальной системы с учетом, что 
        /// что скорости прецессии восходящих узлов орбит на всех ярусах одинаковы.
        /// </summary>
        double CalculateOrbitalSystemEraTier()
        {
            double ret = 0;
            int n = _Tiers[0].Orbit.NDay;
            int composition = n;
            double effectiveDay = _Tiers[0].Orbit.EraTier / n;
            for (int i = 1; i < Tiers.Count; i++)
            {
                int k = _Tiers[i].Orbit.NDay;
                n = MathEx.NOD(n, k);
                composition *= k;
            }
            if (_Tiers.Count == 1) n = 1;
            ret = (composition / n) * effectiveDay;
            return ret;
        }
    }
}
