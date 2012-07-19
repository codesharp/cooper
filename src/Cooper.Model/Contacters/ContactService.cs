using System;
using System.Collections.Generic;

namespace Taobao.Cooper.Model
{
    public class ContactService : IContactService
    {
        private IContactRepository _contactRepository;

        public ContactService(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        public IEnumerable<Contacter> GetAccountContacts(Account account)
        {
            return _contactRepository.GetContactsByAccount(account);
        }

        public void CreateContacter(Contacter contacter)
        {
            _contactRepository.Add(contacter);
        }

        public void UpdateContacter(Contacter contacter)
        {
            _contactRepository.Update(contacter);
        }

        public Contacter GetContacterById(int contacterId)
        {
            return _contactRepository.FindBy(contacterId);
        }

        public void DeleteContacter(Contacter contacter)
        {
            _contactRepository.Remove(contacter);
        }
    }
}