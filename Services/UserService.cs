using Microsoft.EntityFrameworkCore;
using Novati.API.Common;
using Novati.API.Common.Exceptions;
using Novati.API.Common.Paginacao;
using Novati.API.Dtos.Users;
using Novati.API.Mappers;
using Novati.API.Models.Enums;
using Novati.API.Repositories.Interfaces;
using Novati.API.Services.Interfaces;

namespace Novati.API.Services;

/// <summary>Regras de negócio de utilizadores.</summary>
public class UserService(
    IUserRepository users,
    IFicheiroRepository ficheiros,
    IUnitOfWork uow) : IUserService
{
    // Password inicial para novos utilizadores criados via API
    private const string DefaultPassword = "novati123";

    public async Task<UserDto> GetMeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId, ct)
                   ?? throw new NotFoundException("Utilizador não encontrado.");
        return UserMapper.ToDto(user);
    }

    public async Task<List<UserDto>> GetDirectoryAsync(CancellationToken ct = default)
    {
        var all = await users.GetAllAsync(ct);
        return all.Select(UserMapper.ToDto).ToList();
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var all = await users.GetAllAsync(ct);
        return all.Select(UserMapper.ToDto).ToList();
    }

    public async Task<PaginaResultado<UserDto>> GetPaginaAsync(PaginacaoQuery paginacao, string? role, CancellationToken ct = default)
    {
        var filtroRole = FiltroEnum.Ler<Role>(role, "role");
        return (await users.GetPaginaAsync(paginacao, filtroRole, ct)).Map(UserMapper.ToDto);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        // Validar role
        if (!Enum.TryParse<Role>(request.Role, out var role))
            throw new BusinessRuleException("Role inválida.");

        // Email duplicado?
        if (await users.EmailExistsAsync(request.Email, null, ct))
            throw new ConflictException("Já existe um utilizador com este email.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword);
        var user = UserMapper.ToEntity(request, role, passwordHash);

        await users.AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);

        return UserMapper.ToDto(user);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(id, ct)
                   ?? throw new NotFoundException("Utilizador não encontrado.");

        if (!Enum.TryParse<Role>(request.Role, out var role))
            throw new BusinessRuleException("Role inválida.");

        if (await users.EmailExistsAsync(request.Email, id, ct))
            throw new ConflictException("Já existe um utilizador com este email.");

        UserMapper.ApplyUpdate(user, request, role);

        users.Update(user);
        await uow.SaveChangesAsync(ct);

        return UserMapper.ToDto(user);
    }

    public async Task DeleteAsync(Guid id, Guid currentUserId, CancellationToken ct = default)
    {
        if (id == currentUserId)
            throw new BusinessRuleException("Não podes apagar-te a ti próprio.");

        var user = await users.GetByIdAsync(id, ct)
                   ?? throw new NotFoundException("Utilizador não encontrado.");

        try
        {
            users.Remove(user);
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new ConflictException("Não é possível remover: o utilizador tem registos associados.");
        }
    }

    public async Task<UserDto> UpdateMeAsync(Guid userId, UpdateMeRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId, ct)
                   ?? throw new NotFoundException("Utilizador não encontrado.");

        var nome = request.Nome.Trim();
        var email = request.Email.Trim();
        if (nome.Length == 0)
            throw new BusinessRuleException("O nome é obrigatório.");

        if (await users.EmailExistsAsync(email, userId, ct))
            throw new ConflictException("Já existe um utilizador com este email.");

        user.Nome = nome;
        user.Email = email;

        users.Update(user);
        await uow.SaveChangesAsync(ct);

        return UserMapper.ToDto(user);
    }

    public async Task UpdatePasswordAsync(Guid userId, UpdatePasswordRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId, ct)
                   ?? throw new NotFoundException("Utilizador não encontrado.");

        // 400 e não 401: a sessão é válida, o que está errado é o campo do formulário.
        if (!BCrypt.Net.BCrypt.Verify(request.PasswordAtual, user.PasswordHash))
            throw new BusinessRuleException("A password atual não está correta.", "PASSWORD_ATUAL_ERRADA");

        var falhas = PasswordForte.Falhas(request.NovaPassword);
        if (falhas.Count > 0)
            throw new BusinessRuleException($"A nova password é fraca. Falta: {string.Join(", ", falhas)}.", "PASSWORD_FRACA");

        if (request.NovaPassword == request.PasswordAtual)
            throw new BusinessRuleException("A nova password tem de ser diferente da atual.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NovaPassword);

        users.Update(user);
        await uow.SaveChangesAsync(ct);
    }

    public async Task<UserDto> UpdateSignatureAsync(Guid userId, SignatureRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByIdAsync(userId, ct)
                   ?? throw new NotFoundException("Utilizador não encontrado.");

        if (request.FicheiroId is { } ficheiroId)
        {
            var ficheiro = await ficheiros.GetByIdAsync(ficheiroId, ct)
                           ?? throw new NotFoundException("Ficheiro da assinatura não encontrado.");

            // Só a própria pessoa pode usar como assinatura uma imagem que ela carregou.
            if (ficheiro.CriadoPorId != userId)
                throw new ForbiddenException("Só pode usar como assinatura um ficheiro carregado por si.");
            if (!ficheiro.ContentType.StartsWith("image/"))
                throw new BusinessRuleException("A assinatura tem de ser uma imagem.");
        }

        // A imagem anterior não é apagada: relatórios já finalizados continuam a apontar para ela.
        user.AssinaturaFicheiroId = request.FicheiroId;

        users.Update(user);
        await uow.SaveChangesAsync(ct);

        return UserMapper.ToDto(user);
    }
}