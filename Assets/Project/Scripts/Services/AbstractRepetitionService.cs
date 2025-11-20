using System;
using Zenject;

namespace Chang.Services
{
    public class AbstractRepetitionService : IDisposable
    {
        protected const float MarkWeight = 0.25f;
        protected const float SequenceWeight = 0.4f;
        protected const float TimeWeight = 0.015f;

        protected readonly ProfileService ProfileService;
        
        [Inject]
        public AbstractRepetitionService(ProfileService profileService)
        {
            ProfileService = profileService;
        }
        
        public void Dispose()
        {
        }
    }
}