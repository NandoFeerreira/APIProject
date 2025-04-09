param (
    [string]$MigrationName = "AutoMigration_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
)

Write-Host "Verificando alterações no modelo de dados..." -ForegroundColor Cyan

# Verificar se há alterações pendentes
$pendingChanges = dotnet ef migrations list --project .\APIProject.Infrastructure --startup-project .\APIProject.API --no-build | Select-String "[ ]"

if ($pendingChanges) {
    Write-Host "Há migrações pendentes que precisam ser aplicadas:" -ForegroundColor Yellow
    $pendingChanges
    
    $applyPending = Read-Host "Deseja aplicar as migrações pendentes? (S/N)"
    if ($applyPending -eq "S" -or $applyPending -eq "s") {
        Write-Host "Aplicando migrações pendentes..." -ForegroundColor Cyan
        dotnet ef database update --project .\APIProject.Infrastructure --startup-project .\APIProject.API
    }
} else {
    # Verificar se há alterações no modelo
    Write-Host "Verificando se há alterações no modelo que precisam de novas migrations..." -ForegroundColor Cyan
    
    # Tentar adicionar uma migration temporária para ver se há alterações
    $tempMigrationName = "TempMigration_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    $result = dotnet ef migrations add $tempMigrationName --project .\APIProject.Infrastructure --startup-project .\APIProject.API --no-build
    
    # Se a migration temporária foi criada com alterações
    if ($result -match "Done." -and -not ($result -match "No changes")) {
        # Remover a migration temporária
        dotnet ef migrations remove --project .\APIProject.Infrastructure --startup-project .\APIProject.API --no-build
        
        # Perguntar se deseja criar uma nova migration
        $createNew = Read-Host "Há alterações no modelo que precisam de uma nova migration. Deseja criar uma nova migration? (S/N)"
        if ($createNew -eq "S" -or $createNew -eq "s") {
            # Solicitar um nome para a migration se não foi fornecido
            if ($MigrationName -eq "AutoMigration_$(Get-Date -Format 'yyyyMMdd_HHmmss')") {
                $customName = Read-Host "Digite um nome para a migration (ou pressione Enter para usar o nome padrão)"
                if ($customName) {
                    $MigrationName = $customName
                }
            }
            
            Write-Host "Criando nova migration '$MigrationName'..." -ForegroundColor Cyan
            dotnet ef migrations add $MigrationName --project .\APIProject.Infrastructure --startup-project .\APIProject.API
            
            # Perguntar se deseja aplicar a migration
            $applyNew = Read-Host "Deseja aplicar a nova migration? (S/N)"
            if ($applyNew -eq "S" -or $applyNew -eq "s") {
                Write-Host "Aplicando a nova migration..." -ForegroundColor Cyan
                dotnet ef database update --project .\APIProject.Infrastructure --startup-project .\APIProject.API
            }
        }
    } else {
        # Se não há alterações, remover a migration temporária se foi criada
        if ($result -match "Done.") {
            dotnet ef migrations remove --project .\APIProject.Infrastructure --startup-project .\APIProject.API --no-build
        }
        
        Write-Host "Não há alterações no modelo que precisem de uma nova migration." -ForegroundColor Green
    }
}

Write-Host "Processo concluído." -ForegroundColor Green
