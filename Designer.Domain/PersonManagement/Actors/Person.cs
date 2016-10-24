﻿using System;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.PersonManagement.Actors
{
    public class Person : DurableActor<int>
    {
        public Person(int id) : base(id)
        {
            Command<AddLanguage>(l => AddLanguage(l));
            Command<RemoveLanguage>(l => RemoveLanguage(l));
            Command<UpdateAddress>(a => UpdateAddress(a));

            Recover<PersonRegistered>(p => PersonRegistered(p));
            Recover<LanguageAdded>(l => LanguageAdded(l));
            Recover<LanguageRemoved>(l => LanguageRemoved(l));
            Recover<AddressUpdated>(a => AddressUpdated(a));
        }

        #region command handlers
        private void AddLanguage(AddLanguage addLanguage)
        {
        }

        private void RemoveLanguage(RemoveLanguage removeLanguage)
        {
        }

        private void UpdateAddress(UpdateAddress updateAddress)
        {
        }
        #endregion

        #region event handlers
        private void PersonRegistered(PersonRegistered personRegistered)
        {
            // TODO: set all info from registration, ID is alread set via construction
        }

        private void LanguageAdded(LanguageAdded languageAdded)
        {
        }

        private void LanguageRemoved(LanguageRemoved languageRemoved)
        {
        }

        private void AddressUpdated(AddressUpdated addressUpdated)
        {
        }
        #endregion
    }
}
