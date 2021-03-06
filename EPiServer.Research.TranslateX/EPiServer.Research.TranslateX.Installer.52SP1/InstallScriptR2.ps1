﻿# Module installer for R2. To make changes in the file, you have to remove the signature at the bottom of the script
param($destinationFolder = $null, $applicationPath = $null, $SiteUrl = $null)

$ErrorActionPreference="Stop"
$VerbosePreference = "Continue"

# Get the version location path of EPiServer in order to find resources
$epiVersionPath = Get-EPiVersionPath
$epiVersion = Get-EPiVersion

# Get the version location path of EPiServer in order to find resources
$CurrentDir = Split-Path -Parent $MyInvocation.MyCommand.Path
[array]$arrCurrentDir = $CurrentDir.Split("\")
$projectname = $arrCurrentDir[$arrCurrentDir.Length-1]

$sourceTemplateFolder =  Join-Path $CurrentDir "ApplicationFiles"
$proceedWithInstall = $true

# The wizard is used for progress messages so load it even if we don't need to display it
[void][System.Reflection.Assembly]::Load("EPiServerInstall.Wizard, Version=$epiVersion, Culture=neutral, PublicKeyToken=8fe83dea738b45b7")
$wizard = New-Object EPiServer.Install.Wizard.Wizards.SelectWebApplicationWizard

if ($destinationFolder -eq $null)
{	
	$wizard.EPiServerVersionToDisplay = $(Get-EPiVersion)
	$wizard.UserInstructions = "Select the EPiServer Web Application to install $projectname in"
	$wizard.Height = 625
	$wizard.Title = "Install $projectname"
	$proceedWithInstall = $wizard.Show()
	
	if ($proceedWithInstall)
	{
		$applicationPath = $wizard.SelectedApplication.ApplicationName
		$destinationFolder = $wizard.SelectedApplication.PhysicalPath
		$SiteUrl = $wizard.SelectedApplication.SiteUrl
	}
}

if ($proceedWithInstall)
{
	$inTransaction = Get-EPiIsBulkInstalling

	if($inTransaction -eq $false)
	{
		Begin-EPiBulkInstall
	}

	Set-EPiProgressStatus -status $wizard.Resources.GetString("ProgressStatusCopyingFiles")

	# Really, what else is there to do?
	Copy-EPiFiles -SourceDirectoryPath $sourceTemplateFolder -DestinationDirectoryPath $destinationFolder -Recursive -ProgressPercent 60

	# Except delete the powershell script file (this file) from the destination as it will have been copied with
	Remove-EPiFiles -DirectoryPath $destinationFolder -Include "*.ps1" 	

	# Update the WebServer settings in the Visual Studio project file to match the IIS configuration of the site.
	foreach($sourceProjectFile in [System.IO.Directory]::GetFiles($sourceTemplateFolder, "*.*proj", [System.IO.SearchOption]::AllDirectories))
	{
		$targetProjectFile = Join-Path $destinationFolder (Split-Path -Path $sourceProjectFile -Leaf)
		$doc = [xml](Get-Content $sourceProjectFile)			
		$doc.Project.ProjectExtensions.VisualStudio.FlavorProperties.WebProjectProperties.IISUrl = $SiteUrl.ToString()
		$doc.Save($targetProjectFile)
	}

	Set-EPiProgressStatus -status $wizard.Resources.GetString("ProgressStatusInstallingImportPackage")
	
	foreach($importFile in [System.IO.Directory]::GetFiles($CurrentDir, "*.episerverdata", [System.IO.SearchOption]::AllDirectories))
	{
		# Import episerver data
		# To update web.config's start page reference to the imported package add
		# the -updateStartPageAttribute switch to the import command
		Add-EPiServerData $importFile $destinationFolder $applicationPath
	}

	Add-EPiProgressPercentComplete -percent 35

	$webXmlUpdate = Join-Path $CurrentDir "web.config.xmlupdate"
	if ([System.IO.File]::Exists($webXmlUpdate))
	{
		Update-EPiXmlFile -TargetFilePath (Join-Path $destinationFolder "web.config") -ModificationFilePath $webXmlUpdate
	}

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
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUnixBt7XBCtbBIAmJnN28+zP1
# iOmgghD3MIIDejCCAmKgAwIBAgIQOCXX+vhhr570kOcmtdZa1TANBgkqhkiG9w0B
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
# KoZIhvcNAQkEMRYEFG/LYc3iH+N1wC60/MvXkLaa5MF6MA0GCSqGSIb3DQEBAQUA
# BIGAUVe82eLwCY/DOy1O1TKZXVm04dC/dI5mU4hMuj68tnwhN1jLfyP2RCkcqvIE
# hyRNgxpEU7JN0vwZsHi96OznoxdneADyltkwJ3M/cD8IrTXskFF3T35yPgx2owoj
# Zsyv7tkioyrhbMdsYK374Gu1qjcNps9oC93XBfohKJORuxihggF/MIIBewYJKoZI
# hvcNAQkGMYIBbDCCAWgCAQEwZzBTMQswCQYDVQQGEwJVUzEXMBUGA1UEChMOVmVy
# aVNpZ24sIEluYy4xKzApBgNVBAMTIlZlcmlTaWduIFRpbWUgU3RhbXBpbmcgU2Vy
# dmljZXMgQ0ECEDgl1/r4Ya+e9JDnJrXWWtUwCQYFKw4DAhoFAKBdMBgGCSqGSIb3
# DQEJAzELBgkqhkiG9w0BBwEwHAYJKoZIhvcNAQkFMQ8XDTA5MDIxMTE1NTcxNlow
# IwYJKoZIhvcNAQkEMRYEFO27pAQK8bN7VAxsR1ohREmF7P0iMA0GCSqGSIb3DQEB
# AQUABIGAu0raqLOKsxnNEodxB2mMcZMo5l2dn33QB7uimfPvHtOvAoCgwbfYF5zt
# 5+ZmVvmFmxqDIilKiiUmDbOb12k56hkawQDZP3+QA8sWNHT1hAkyt2ExUP+b6QMe
# 2Bmj8TtERXp2mm8yO1UJ2Ivi4uv4Xpw+9dLV1aBtXNdR0hjP3aA=
# SIG # End signature block
