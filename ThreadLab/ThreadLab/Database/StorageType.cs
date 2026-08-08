namespace ThreadLab.Database
{
    internal enum StorageType : byte
    {
        SQLServerEfDbContext,
        InMemoryEfDbContext,
        RawMemory
    }
}
