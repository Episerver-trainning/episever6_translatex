# To make changes in the file, you have to remove the signature at the bottom of the script
param($selectedWebApplication = $null, $features = @{applicationFiles=$true; importContent=$true})

trap [Exception]
{
	echo ""
	echo "An unhandled error has occured:"
	echo $_.Exception
	echo "When executing"
	echo $_.InvocationInfo.PositionMessage
	echo ""	$inTransaction = Get-EPiIsBulkInstalling

	$inTransaction = Get-EPiIsBulkInstalling

	if($inTransaction -eq $true)
	{
		Rollback-EPiBulkInstall
	}	
	
	break
}

$VerbosePreference = "Continue"
$ErrorActionPreference = "Continue"
$Product = "CMS"

# Load the EPiServer Common snapin if required
$snapIn = Get-PSSnapin -Name EPiServer.Install.Common.1 -ErrorAction SilentlyContinue
if ($snapIn -eq $null)
{
	Add-PSSnapin EPiServer.Install.Common.1
}

# Get the version location path of EPiServer in order to find resources
$CurrentDir = Split-Path -Parent $MyInvocation.MyCommand.Path
[array]$arrCurrentDir = $CurrentDir.Split("\")
$projectname = $arrCurrentDir[$arrCurrentDir.Length-1]

$epiVersion = Get-EPiVersion -MapPath $CurrentDir

# Load the EPiServer CMS snapin if required
$snapIn = Get-PSSnapin -Name EPiServer.Install.CMS.$epiVersion -ErrorAction SilentlyContinue
if ($snapIn -eq $null)
{
	#Remove-PSSnapin EPiServer.Install.Common.1
	Add-PSSnapin EPiServer.Install.CMS.$epiVersion
}

$proceedWithInstall = $true

# The wizard is used for progress messages so load it even if we don't need to display it
$wizard = New-EPiCMSWizard "InstallModuleWizard" ($MyInvocation.MyCommand.Path)

$wizard.Features = $features

$epiProductInfo = Get-EPiProductInformation -ProductName $Product -ProductVersion $epiVersion
if (!$epiProductInfo.IsInstalled)
{
	throw(New-Object ApplicationException($wizard.Resources.GetString("ErrorInstallationDirectoryNotFound")))
}

$sourceTemplateFolder = Join-Path $CurrentDir "ApplicationFiles"

if ($selectedWebApplication -eq $null)
{	
	$wizard.CMSVersions = $($epiVersion)
	$wizard.ValidateDatabaseConnection = $true
	$wizard.Title = $projectname
	$proceedWithInstall = $wizard.Show()
	
	if ($proceedWithInstall)
	{
		$selectedWebApplication = $wizard.SelectedApplication		
	}
}

if ($proceedWithInstall)
{
	# Read back features
	$features = $wizard.Features
	
	$inTransaction = Get-EPiIsBulkInstalling

	if($inTransaction -eq $false)
	{
		Begin-EPiBulkInstall
	}

	if ($features.applicationFiles)
	{
		Set-EPiProgressStatus -status $wizard.Resources.GetString("ProgressStatusCopyingFiles")

		# Really, what else is there to do?
		Copy-EPiFiles -SourceDirectoryPath $sourceTemplateFolder -DestinationDirectoryPath $selectedWebApplication.PhysicalPath -Exclude "*.ps1" -Recursive -ProgressPercent 60
			
		# Update the WebServer settings in the Visual Studio project file to match the IIS configuration of the site.
		foreach($sourceProjectFile in [System.IO.Directory]::GetFiles($sourceTemplateFolder, "*.*proj", [System.IO.SearchOption]::AllDirectories))
		{
			$targetProjectFile = Join-Path $selectedWebApplication.PhysicalPath (Split-Path -Path $sourceProjectFile -Leaf)
			$doc = [xml](Get-Content $sourceProjectFile)			
			$doc.Project.ProjectExtensions.VisualStudio.FlavorProperties.WebProjectProperties.IISUrl = $selectedWebApplication.SiteUrl.ToString()
			$doc.Save($targetProjectFile)
		}
	}

	Set-EPiProgressStatus -status $wizard.Resources.GetString("ProgressStatusInstallingImportPackage")

	if ($features.importContent)
	{
		# Import episerver data and udpate start page
		# Loop through the current
		foreach($importFile in [System.IO.Directory]::GetFiles($CurrentDir, "*.episerverdata", [System.IO.SearchOption]::AllDirectories))
		{
			Add-EPiServerData $importFile $selectedWebApplication.PhysicalPath $selectedWebApplication.ApplicationName -updateStartPageAttribute
			Add-EPiProgressPercentComplete -percent 15
		}
	}
	$dbConnection = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($wizard.SelectedApplication.SiteDbConnectionString)
	$file = [System.IO.Path]::Combine($sourceTemplateFolder, "sql.txt")
	Execute-EPiSqlSvrScript -ScriptPath $file -SqlServerName $dbConnection.DataSource -DatabaseName $dbConnection.InitialCatalog -LoginName $dbConnection.UserID -LoginPassword $dbConnection.Password	
	
	$webConfigUpdateFile = [System.IO.Path]::Combine($sourceTemplateFolder, "web.config.xmlupdate")
	$targetApplicationFolder = $wizard.SelectedApplication.PhysicalPath
	$targetWebConfigFile = ([System.IO.Path]::Combine($targetApplicationFolder, 'web.config'))

	Update-EPiXmlFile -TargetFilePath $targetWebConfigFile -ModificationFilePath $webConfigUpdateFile

	if($inTransaction -eq $false)
	{
		Commit-EPiBulkInstall
		Add-EPiProgressPercentComplete -percent 5
	}

	Set-EPiProgressStatus -status $wizard.Resources.GetString("ProgressStatusFinished")
}

# SIG # Begin signature block
# MIIU8gYJKoZIhvcNAQcCoIIU4zCCFN8CAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUBjQwUl79KqGPLEuxfymE2KUv
# 0rmgghD3MIIDejCCAmKgAwIBAgIQOCXX+vhhr570kOcmtdZa1TANBgkqhkiG9w0B
# AQUFADBTMQswCQYDVQQGEwJVUzEXMBUGA1UEChMOVmVyaVNpZ24sIEluYy4xKzAp
# BgNVBAMTIlZlcmlTaWduIFRpbWUgU3RhbXBpbmcgU2VydmljZXMgQ0EwHhcNMDcw
# NjE1MDAwMDAwWhcNMTIwNjE0MjM1OTU5WjBcMQswCQYDVQQGEwJVUzEXMBUGA1UE
# ChMOVmVyaVNpZ24sIEluYy4xNDAyBgNVBAMTK1ZlcmlTaWduIFRpbWUgU3RhbXBp
# bmcgU2VydmljZXMgU2lnbmVyIC0gRzIwgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJ
# AoGBAMS18lIVvIiGYCkWSlsvS5Frh5HzNVRYNerRNl5iTVJRNHHCe2YdicjdKsRq
# CvY32Zh0kfaSrrC1dpbxqUpjRUcuawuSTksrjO5YSovUB+QaLPiCqljZzULzLcB1
# 3o2rx44dmmxMCJUe3tvvZ+FywknCnmA84eK+FqNjeGkUe60tAgMBAAGjgcQwgcEw
# NAYIKwYBBQUHAQEEKDAmMCQGCCsGAQUFBzABhhhodHRwOi8vb2NzcC52ZXJpc2ln
# bi5jb20wDAYDVR0TAQH/BAIwADAzBgNVHR8ELDAqMCigJqAkhiJodHRwOi8vY3Js
# LnZlcmlzaWduLmNvbS90c3MtY2EuY3JsMBYGA1UdJQEB/wQMMAoGCCsGAQUFBwMI
# MA4GA1UdDwEB/wQEAwIGwDAeBgNVHREEFzAVpBMwETEPMA0GA1UEAxMGVFNBMS0y
# MA0GCSqGSIb3DQEBBQUAA4IBAQBQxUvIJIDf5A0kwt4asaECoaaCLQyDFYE3CoIO
# LLBaF2G12AX+iNvxkZGzVhpApuuSvjg5sHU2dDqYT+Q3upmJypVCHbC5x6CNV+D6
# 1WQEQjVOAdEzohfITaonx/LhhkwCOE2DeMb8U+Dr4AaH3aSWnl4MmOKlvr+ChcNg
# 4d+tKNjHpUtk2scbW72sOQjVOCKhM4sviprrvAchP0RBCQe1ZRwkvEjTRIDroc/J
# ArQUz1THFqOAXPl5Pl1yfYgXnixDospTzn099io6uE+UAKVtCoNd+V5T9BizVw9w
# w/v1rZWgDhfexBaAYMkPK26GBPHr9Hgn0QXF7jRbXrlJMvIzMIIDxDCCAy2gAwIB
# AgIQR78Zld+NUkZD99ttSA0xpDANBgkqhkiG9w0BAQUFADCBizELMAkGA1UEBhMC
# WkExFTATBgNVBAgTDFdlc3Rlcm4gQ2FwZTEUMBIGA1UEBxMLRHVyYmFudmlsbGUx
# DzANBgNVBAoTBlRoYXd0ZTEdMBsGA1UECxMUVGhhd3RlIENlcnRpZmljYXRpb24x
# HzAdBgNVBAMTFlRoYXd0ZSBUaW1lc3RhbXBpbmcgQ0EwHhcNMDMxMjA0MDAwMDAw
# WhcNMTMxMjAzMjM1OTU5WjBTMQswCQYDVQQGEwJVUzEXMBUGA1UEChMOVmVyaVNp
# Z24sIEluYy4xKzApBgNVBAMTIlZlcmlTaWduIFRpbWUgU3RhbXBpbmcgU2Vydmlj
# ZXMgQ0EwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCpyrKkzM0grwp9
# iayHdfC0TvHfwQ+/Z2G9o2Qc2rv5yjOrhDCJWH6M22vdNp4Pv9HsePJ3pn5vPL+T
# rw26aPRslMq9Ui2rSD31ttVdXxsCn/ovax6k96OaphrIAuF/TFLjDmDsQBx+uQ3e
# P8e034e9X3pqMS4DmYETqEcgzjFzDVctzXg0M5USmRK53mgvqubjwoqMKsOLIYdm
# vYNYV291vzyqJoddyhAVPJ+E6lTBCm7E/sVK3bkHEZcifNs+J9EeeOyfMcnx5iIZ
# 28SzR0OaGl+gHpDkXvXufPF9q2IBj/VNC97QIlaolc2uiHau7roN8+RN2aD7aKCu
# FDuzh8G7AgMBAAGjgdswgdgwNAYIKwYBBQUHAQEEKDAmMCQGCCsGAQUFBzABhhho
# dHRwOi8vb2NzcC52ZXJpc2lnbi5jb20wEgYDVR0TAQH/BAgwBgEB/wIBADBBBgNV
# HR8EOjA4MDagNKAyhjBodHRwOi8vY3JsLnZlcmlzaWduLmNvbS9UaGF3dGVUaW1l
# c3RhbXBpbmdDQS5jcmwwEwYDVR0lBAwwCgYIKwYBBQUHAwgwDgYDVR0PAQH/BAQD
# AgEGMCQGA1UdEQQdMBukGTAXMRUwEwYDVQQDEwxUU0EyMDQ4LTEtNTMwDQYJKoZI
# hvcNAQEFBQADgYEASmv56ljCRBwxiXmZK5a/gqwB1hxMzbCKWG7fCCmjXsjKkxPn
# BFIN70cnLwA4sOTJk06a1CJiFfc/NyFPcDGA8Ys4h7Po6JcA/s9Vlk4k0qknTnqu
# t2FB8yrO58nZXt27K4U+tZ212eFX/760xX71zwye8Jf+K9M7UhsbOCf3P0owggS/
# MIIEKKADAgECAhBBkaFaOXjfz0llZjgdTHXCMA0GCSqGSIb3DQEBBQUAMF8xCzAJ
# BgNVBAYTAlVTMRcwFQYDVQQKEw5WZXJpU2lnbiwgSW5jLjE3MDUGA1UECxMuQ2xh
# c3MgMyBQdWJsaWMgUHJpbWFyeSBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eTAeFw0w
# NDA3MTYwMDAwMDBaFw0xNDA3MTUyMzU5NTlaMIG0MQswCQYDVQQGEwJVUzEXMBUG
# A1UEChMOVmVyaVNpZ24sIEluYy4xHzAdBgNVBAsTFlZlcmlTaWduIFRydXN0IE5l
# dHdvcmsxOzA5BgNVBAsTMlRlcm1zIG9mIHVzZSBhdCBodHRwczovL3d3dy52ZXJp
# c2lnbi5jb20vcnBhIChjKTA0MS4wLAYDVQQDEyVWZXJpU2lnbiBDbGFzcyAzIENv
# ZGUgU2lnbmluZyAyMDA0IENBMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKC
# AQEAvrzuvH7vg+vgN0/7AxA4vgjSjH2d+pJ/GQzCa+5CUoze0xxIEyXqwWN6+VFl
# 7tOqO/XwlJwr+/Jm1CTa9/Wfbhk5NrzQo3YIHiInJGw4kSfihEmuG4qh/SWCLBAw
# 6HGrKOh3SlHx7M348FTUb8DjbQqP2dhkjWOyLU4n9oUO/m3jKZnihUd8LYZ/6FeP
# rWfCMzKREyD8qSMUmm3ChEt2aATVcSxdIfqIDSb9Hy2RK+cBVU3ybTUogt/Za1y2
# 1tmqgf1fzYO6Y53QIvypO0Jpso46tby0ng9exOosgoso/VMIlt21ASDR+aUY58Du
# UXA34bYFSFJIbzjqw+hse0SEuwIDAQABo4IBoDCCAZwwEgYDVR0TAQH/BAgwBgEB
# /wIBADBEBgNVHSAEPTA7MDkGC2CGSAGG+EUBBxcDMCowKAYIKwYBBQUHAgEWHGh0
# dHBzOi8vd3d3LnZlcmlzaWduLmNvbS9ycGEwMQYDVR0fBCowKDAmoCSgIoYgaHR0
# cDovL2NybC52ZXJpc2lnbi5jb20vcGNhMy5jcmwwHQYDVR0lBBYwFAYIKwYBBQUH
# AwIGCCsGAQUFBwMDMA4GA1UdDwEB/wQEAwIBBjARBglghkgBhvhCAQEEBAMCAAEw
# KQYDVR0RBCIwIKQeMBwxGjAYBgNVBAMTEUNsYXNzM0NBMjA0OC0xLTQzMB0GA1Ud
# DgQWBBQI9VHo+/49PWQ2fGjPW3io37nFNzCBgAYDVR0jBHkwd6FjpGEwXzELMAkG
# A1UEBhMCVVMxFzAVBgNVBAoTDlZlcmlTaWduLCBJbmMuMTcwNQYDVQQLEy5DbGFz
# cyAzIFB1YmxpYyBQcmltYXJ5IENlcnRpZmljYXRpb24gQXV0aG9yaXR5ghBwuuQd
# ENkpNLY4ynsDzLq/MA0GCSqGSIb3DQEBBQUAA4GBAK46F7hKe1X6ZFXsQKTtSUGQ
# mZyJvK8uHcp4I/kcGQ9/62i8MtmION7cP9OJtD+xgpbxpFq67S4m0958AW4ACgCk
# BpIRSAlA+RwYeWcjJOC71eFQrhv1Dt3gLoHNgKNsUk+RdVWKuiLy0upBdYgvY1V9
# HlRalVnK2TSBwF9e9nq1MIIE6jCCA9KgAwIBAgIQcSAFVHPmXMe+OxrwtJAQhzAN
# BgkqhkiG9w0BAQUFADCBtDELMAkGA1UEBhMCVVMxFzAVBgNVBAoTDlZlcmlTaWdu
# LCBJbmMuMR8wHQYDVQQLExZWZXJpU2lnbiBUcnVzdCBOZXR3b3JrMTswOQYDVQQL
# EzJUZXJtcyBvZiB1c2UgYXQgaHR0cHM6Ly93d3cudmVyaXNpZ24uY29tL3JwYSAo
# YykwNDEuMCwGA1UEAxMlVmVyaVNpZ24gQ2xhc3MgMyBDb2RlIFNpZ25pbmcgMjAw
# NCBDQTAeFw0wNzAzMTQwMDAwMDBaFw0xMDAzMTMyMzU5NTlaMIGtMQswCQYDVQQG
# EwJTRTESMBAGA1UECBMJU3RvY2tob2xtMQ4wDAYDVQQHEwVLaXN0YTEVMBMGA1UE
# ChQMRVBpU2VydmVyIEFCMT4wPAYDVQQLEzVEaWdpdGFsIElEIENsYXNzIDMgLSBN
# aWNyb3NvZnQgU29mdHdhcmUgVmFsaWRhdGlvbiB2MjEMMAoGA1UECxQDQ01TMRUw
# EwYDVQQDFAxFUGlTZXJ2ZXIgQUIwgZ8wDQYJKoZIhvcNAQEBBQADgY0AMIGJAoGB
# AJcIjZT6qZPtQBphnmMRx1te/GkZTmTQ3703w/vBiGCDIey9/YwucIH23shM58fG
# jTyg1AHbQ8kIisr+3Z+kasPNsB2QMCl7kou/L22OGOdWIwSwceP1xUHNI/Dhm+Ar
# uBIQ9OpAqBUeUsC+r8S25+yQMNVokSxqQ5EoHcP9tv5PAgMBAAGjggF/MIIBezAJ
# BgNVHRMEAjAAMA4GA1UdDwEB/wQEAwIHgDBABgNVHR8EOTA3MDWgM6Axhi9odHRw
# Oi8vQ1NDMy0yMDA0LWNybC52ZXJpc2lnbi5jb20vQ1NDMy0yMDA0LmNybDBEBgNV
# HSAEPTA7MDkGC2CGSAGG+EUBBxcDMCowKAYIKwYBBQUHAgEWHGh0dHBzOi8vd3d3
# LnZlcmlzaWduLmNvbS9ycGEwEwYDVR0lBAwwCgYIKwYBBQUHAwMwdQYIKwYBBQUH
# AQEEaTBnMCQGCCsGAQUFBzABhhhodHRwOi8vb2NzcC52ZXJpc2lnbi5jb20wPwYI
# KwYBBQUHMAKGM2h0dHA6Ly9DU0MzLTIwMDQtYWlhLnZlcmlzaWduLmNvbS9DU0Mz
# LTIwMDQtYWlhLmNlcjAfBgNVHSMEGDAWgBQI9VHo+/49PWQ2fGjPW3io37nFNzAR
# BglghkgBhvhCAQEEBAMCBBAwFgYKKwYBBAGCNwIBGwQIMAYBAQABAf8wDQYJKoZI
# hvcNAQEFBQADggEBAGMyN7sOUiRYGmCnB4qek7IBLV2w8i5i3RK3Rt7RY6roImoO
# SOYCLR+Vn5MSuqEnzxtm2thzzX+Lbiecw88/xUVqkT2ThUn6CUwzr3O6XEdxKQxt
# L1flFbreUeeT9CQPzyYzJZvYdLXutRTKr0WyTs3urvuOS/783NZt1aXaSrx5tr0Y
# BpheBEWSK5SHd4xwuZbzU9xW+9/zGbavy8tJUT7GhmhnSBkt8UsZe12uoNW0tNgY
# w9Fy+eaSHqY9DJl9gEZJ7bG5B8cJbybwkin1Ikn9s5n9yIvMiEvcnOf8d4fdy9GD
# Q+0dC3u+o84ZDfjJ3lSJomeV433Apy1MW56c8iYxggNlMIIDYQIBATCByTCBtDEL
# MAkGA1UEBhMCVVMxFzAVBgNVBAoTDlZlcmlTaWduLCBJbmMuMR8wHQYDVQQLExZW
# ZXJpU2lnbiBUcnVzdCBOZXR3b3JrMTswOQYDVQQLEzJUZXJtcyBvZiB1c2UgYXQg
# aHR0cHM6Ly93d3cudmVyaXNpZ24uY29tL3JwYSAoYykwNDEuMCwGA1UEAxMlVmVy
# aVNpZ24gQ2xhc3MgMyBDb2RlIFNpZ25pbmcgMjAwNCBDQQIQcSAFVHPmXMe+Oxrw
# tJAQhzAJBgUrDgMCGgUAoHAwEAYKKwYBBAGCNwIBDDECMAAwGQYJKoZIhvcNAQkD
# MQwGCisGAQQBgjcCAQQwHAYKKwYBBAGCNwIBCzEOMAwGCisGAQQBgjcCARUwIwYJ
# KoZIhvcNAQkEMRYEFNCQHkX2AxQIvWlW4diCnGj1bE/kMA0GCSqGSIb3DQEBAQUA
# BIGAWho7cOz/GuZ47t9Wbdtcr31I3K+rbyflUbRnxicQTFI+HtoXXdnLw/x995oC
# 5cu4LKEVEObYnZDqre0LWSJVegPkNaVtCJlG6JuDnyLHqwISwykJBZaFume7bcB7
# CV/5Pzt5x8ROHYqDrOxGNIWBSqrsrAmayV+MzpTVL28lAkqhggF/MIIBewYJKoZI
# hvcNAQkGMYIBbDCCAWgCAQEwZzBTMQswCQYDVQQGEwJVUzEXMBUGA1UEChMOVmVy
# aVNpZ24sIEluYy4xKzApBgNVBAMTIlZlcmlTaWduIFRpbWUgU3RhbXBpbmcgU2Vy
# dmljZXMgQ0ECEDgl1/r4Ya+e9JDnJrXWWtUwCQYFKw4DAhoFAKBdMBgGCSqGSIb3
# DQEJAzELBgkqhkiG9w0BBwEwHAYJKoZIhvcNAQkFMQ8XDTA5MDMyMDE2NDAzOFow
# IwYJKoZIhvcNAQkEMRYEFCW4BccTZw/YR5XpXmVYz00BxzRtMA0GCSqGSIb3DQEB
# AQUABIGAV4cQf7ZvgjN8CG+9SSeVb+zURB1RJ01F4ILVBizIb/qj7DCTP8/GKteV
# RMuB3WI5qvP+jIQNaiR1N4CLnDrgziaUYoUvfaTYZK6GLVdr4824gBDsGdfHp+4m
# gQUVWU/kxuY1qNM0yd5JLEddN0MPPws/tyT5zDCTZ2N0gNr+75o=
# SIG # End signature block
