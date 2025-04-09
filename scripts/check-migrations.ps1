Write-Host "Verificando migrações pendentes..." -ForegroundColor Cyan

# Verificar se há migrações pendentes
$pendingMigrations = dotnet ef migrations list --project .\APIProject.Infrastructure --startup-project .\APIProject.API --no-build | Select-String "[ ]"

if ($pendingMigrations) {
    Write-Host "Há migrações pendentes que precisam ser aplicadas:" -ForegroundColor Yellow
    $pendingMigrations
    
    $applyPending = Read-Host "Deseja aplicar as migrações pendentes? (S/N)"
    if ($applyPending -eq "S" -or $applyPending -eq "s") {
        Write-Host "Aplicando migrações pendentes..." -ForegroundColor Cyan
        dotnet ef database update --project .\APIProject.Infrastructure --startup-project .\APIProject.API
        Write-Host "Migrações aplicadas com sucesso." -ForegroundColor Green
    }
} else {
    Write-Host "Não há migrações pendentes." -ForegroundColor Green
}

Write-Host "Processo concluído." -ForegroundColor Green
