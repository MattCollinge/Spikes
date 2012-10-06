using System;
using System.Collections.Generic;
using System.Linq;
using CtM.EDA;
using MVT.Domain.Events;
using MVT.Domain.Entities;

namespace MVT.Domain.Aggregates
{
    public class MVTestAR : AggregateRoot
    {
        private Guid _Id;
        private string _TestName;
        private string _PausedBy;
        private Dictionary<string, Variant> _variants = new Dictionary<string, Variant>(5);
        private readonly List<Guid> _successCriteria = new List<Guid>(3);

        public override Guid Id
        {
            get { return _Id; }
        }
        
        public MVTestAR()
        {
        }

        #region Behaviour
      
        public MVTestAR(Guid testId, string testName)
        {
            var @event = new NewMVTestSetup()
                             {
                                 TestId = testId,
                                 TestName = testName
                             };

            ApplyChange(@event);
        }

        public void Pause(string pauseRequestBy)
        {
            var @event = new MVTestPaused()
                             {
                                 TestId=this.Id,
                                 PausedBy=pauseRequestBy 
                             };

            ApplyChange(@event);
        }

        public void SetUpNewVariant(string variantName)
        {
            //TODO: Check Variant hasn't already been added
            var @event = new NewVariantSetup()
                             {
                                 TestId=this.Id,
                                 VariantName= variantName
                             };
        
           ApplyChange(@event);
        }

        public void SetVariantWeightSchedule(Dictionary<string, int> variantWeightSchedule)
        {
            //TODO: Check all variants exist

            if (variantWeightSchedule.Values.Sum() != 100)
            {
                //Duff weighting
                throw new ArgumentException("Variant Weighting don't add up to 100", "variantWeightSchedule");
            }

            var @event = new VariantWeightScheduleApplied()
            {
                TestId = this.Id,
                VariantWeightSchedule = variantWeightSchedule
            };

            ApplyChange(@event);
        }

        public void AssociateSuccesCriteria(Guid successCriteriaId)
        {
           //TODO: Check it's not already associated
            //TODO: Implement ordered success criteria tracking??
            var @event = new SuccessCriteriaAssociated()
                             {
                                 MVTestId = _Id,
                                 SuccessCriteriaId = successCriteriaId
                             };
            ApplyChange(@event);
        }

        #endregion

        #region Event Handling

        private void Apply(NewMVTestSetup e)
        {
            _Id  = e.TestId;
            _TestName = e.TestName;
        }

        private void Apply(MVTestPaused e)
        {
            _PausedBy = e.PausedBy;
        }

        private void Apply(NewVariantSetup e)
        {
            _variants.Add(e.VariantName, new Variant()
                                             {
                                                 variantName = e.VariantName
                                             });
        }

        private void Apply(VariantWeightScheduleApplied e)
        {
            foreach (var variantWeight in e.VariantWeightSchedule)
            {
                _variants[variantWeight.Key].weight = variantWeight.Value;
            }
            
        }

        private void Apply(SuccessCriteriaAssociated e)
        {
            _successCriteria.Add(e.SuccessCriteriaId);
        }

        #endregion
    }

    public class SuccessCriteriaAssociated : Event
    {
        public Guid MVTestId;
        public Guid SuccessCriteriaId;
    }
}