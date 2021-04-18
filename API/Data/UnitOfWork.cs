using System.Threading.Tasks;
using API.Interfaces;
using AutoMapper;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UnitOfWork(DataContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public IUserRepository userRepository => new UserRepository(_context,_mapper);

        public IMessageRepository messageRepository =>new MessageRepository(_context, _mapper);

        public ILikesRepository likesRepository => new LikesRepository(_context);

        public async Task<bool> Completed()
        {
           return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
           return _context.ChangeTracker.HasChanges();
        }
    }
}