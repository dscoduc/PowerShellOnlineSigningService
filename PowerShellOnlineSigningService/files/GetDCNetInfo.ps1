<#-- Digital Signing requested by DSCOWIN81\Administrator --#>

# ################################################################
<#
 Script      : GetDCNetInfo.ps1
 Description : This script will enumerate all DC's in a forest and 
               retrieve the IP info for each
 Author      : chris@dscoduc.com
 Date        : 7/15/2015
 Version     : V 1.0
 ----------------------------------------------------------------
 Keywords    : DC Net IP Info Forest ActiveDirectory
#>
$Error.Clear()
Set-StrictMode �Version Latest

# Define output file
$outputFile = $MyInvocation.MyCommand.Name.Replace(".ps1", ".log")

# load required module
Import-Module ActiveDirectory -ErrorAction Stop

# Get Forest
$ADForest = Get-ADForest -Current LocalComputer

# Get list of domains in forest
$ADForest.Domains | ForEach-Object {

    # get domain object
    $domain = $_

    # Get list of domain controllers in each domain
    Get-ADGroupMember 'Domain Controllers' -Server $domain | ForEach-Object {

        # get domain controller object
        $domaincontroller = $_

        # get domain controller dnsHostName
        $dcDnsHostName = "$($domaincontroller.name).$domain"

        if(Test-Connection -ComputerName $dcDnsHostName -Count 1 -ea 0) {
            $Networks = Get-WmiObject Win32_NetworkAdapterConfiguration -ComputerName $dcDnsHostName | ? {$_.IPEnabled}
            foreach ($Network in $Networks) {
                $IPAddress  = $Network.IpAddress[0]
                $SubnetMask  = $Network.IPSubnet[0]
                $DefaultGateway = $Network.DefaultIPGateway
                $DNSServers  = $Network.DNSServerSearchOrder
                $IsDHCPEnabled = $false
                If($network.DHCPEnabled) {
                    $IsDHCPEnabled = $true
                }
                $MACAddress  = $Network.MACAddress
                $OutputObj  = New-Object -Type PSObject
                $OutputObj | Add-Member -MemberType NoteProperty -Name ComputerName -Value $dcDnsHostName.ToUpper()
                $OutputObj | Add-Member -MemberType NoteProperty -Name Status -Value "Online"
                $OutputObj | Add-Member -MemberType NoteProperty -Name IPAddress -Value $IPAddress
                $OutputObj | Add-Member -MemberType NoteProperty -Name SubnetMask -Value $SubnetMask
                $OutputObj | Add-Member -MemberType NoteProperty -Name Gateway -Value $DefaultGateway
                $OutputObj | Add-Member -MemberType NoteProperty -Name IsDHCPEnabled -Value $IsDHCPEnabled
                $OutputObj | Add-Member -MemberType NoteProperty -Name DNSServers -Value $DNSServers
                $OutputObj | Add-Member -MemberType NoteProperty -Name MACAddress -Value $MACAddress

                # append output information
                $OutputObj | Out-File -Append $outputFile
            }
        } else {
            $OutputObj  = New-Object -Type PSObject
            $OutputObj | Add-Member -MemberType NoteProperty -Name ComputerName -Value $dcDnsHostName.ToUpper()
            $OutputObj | Add-Member -MemberType NoteProperty -Name Status -Value "Unreachable"
            
            # append output information
            $OutputObj | Out-File -Append $outputFile
        }
    }
}

# SIG # Begin signature block
# MIILCQYJKoZIhvcNAQcCoIIK+jCCCvYCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUtL2BRP1lelH/wKWdJPNONGtE
# GZWgggkpMIIEVjCCAz6gAwIBAgITFwAAALmlrZpGVYHafQAAAAAAuTANBgkqhkiG
# 9w0BAQsFADATMREwDwYDVQQDEwhPUkQxQ0EwMTAeFw0xNTA2MjcwNDUxMTlaFw0x
# NzA2MjcwNTAxMTlaMIGCMRQwEgYKCZImiZPyLGQBGRYEQ09SUDEZMBcGCgmSJomT
# 8ixkARkWCVJBQ0tTUEFDRTERMA8GA1UECxMIQWNjb3VudHMxDDAKBgNVBAsTA0RM
# SDESMBAGA1UECxMJRnVsbC1UaW1lMRowGAYDVQQDExFDaHJpcyBCbGFua2Vuc2hp
# cDCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEAr3/GtfcE8LIjB3XOtRUrv4uh
# 3I8jx/YALmw2b3lLTCpsUbab2fneBEaoxQUzxcMtX7SVDGCFOHNkWUHVVS95pjyy
# 0YDDlriVQxxXpXO/ubicoAsFusI7N7rZjRdfD1vs0KI9Y6Ig2wpgJtma/iDoa8BF
# OtTOs4Ok97pAJntqNLECAwEAAaOCAbUwggGxMDwGCSsGAQQBgjcVBwQvMC0GJSsG
# AQQBgjcVCIP6s2qCr64R7YM3gd31FYOMvicChPTaNoerji0CAWQCAQQwEwYDVR0l
# BAwwCgYIKwYBBQUHAwMwCwYDVR0PBAQDAgeAMBsGCSsGAQQBgjcVCgQOMAwwCgYI
# KwYBBQUHAwMwHQYDVR0OBBYEFIjeg/dMalx3+RsvMZRD2ZW1HaamMB8GA1UdIwQY
# MBaAFNIz84LaMgb7EVHZWqa7PhSme8fxMIG9BggrBgEFBQcBAQSBsDCBrTCBqgYI
# KwYBBQUHMAKGgZ1sZGFwOi8vL0NOPU9SRDFDQTAxLENOPUFJQSxDTj1QdWJsaWMl
# MjBLZXklMjBTZXJ2aWNlcyxDTj1TZXJ2aWNlcyxDTj1Db25maWd1cmF0aW9uLERD
# PVJBQ0tTUEFDRSxEQz1DT1JQP2NBQ2VydGlmaWNhdGU/YmFzZT9vYmplY3RDbGFz
# cz1jZXJ0aWZpY2F0aW9uQXV0aG9yaXR5MDIGA1UdEQQrMCmgJwYKKwYBBAGCNxQC
# A6AZDBdjYmxhbmtlbkBSQUNLU1BBQ0UuQ09SUDANBgkqhkiG9w0BAQsFAAOCAQEA
# gPGNXEFggArgQeD096cS3vzN97G+i6wVJOeMPSz3jOQYheqPiBA3M/Gi16TPxjBC
# HRs1y4VjjFQz2cmM8+w9Bbb5nZSvyFkvNmEYey1Vs4sjxea1la+CxlfdgTfVQRW0
# kA08IHTHEQew1Z8381p6D4hCiExh/l30b8qC2iR/XcsOJTfTK69uIyfwWgJSq6ZT
# yZBfGEw6LFaYzbmiyQbqrAv/2N4mWI1w/eO8wOAipjGwjYs7dCZFDzQsg7+DkCmj
# cDPBGClZLVEtAp2Wv058BWI7AoFUQAhRgEvqcVW81Tyr3/rqhqAQX/3Mac/7SFr6
# QBHqeLyPr8G3k8l/2+G1djCCBMswggKzoAMCAQICAwIEETANBgkqhkiG9w0BAQsF
# ADB0MQswCQYDVQQGEwJVUzEOMAwGA1UECAwFVGV4YXMxFDASBgNVBAcMC1NhbiBB
# bnRvbmlvMSAwHgYDVQQKDBdSYWNrc3BhY2UgSG9zdGluZywgSW5jLjEdMBsGA1UE
# AwwUUmFja3NwYWNlIENBIExldmVsIDEwHhcNMTUwNTA1MTE1NjE2WhcNMjUwNTAy
# MTE1NjE2WjATMREwDwYDVQQDEwhPUkQxQ0EwMTCCASIwDQYJKoZIhvcNAQEBBQAD
# ggEPADCCAQoCggEBAL+tE/NlocKT7Rf6v4UZoI73w/SqRvmAn5KpfaflGQf37l8D
# FSHJDEIUTFcRgOCiYgPFDL8mdhNikT95J9CgpQb/zdZj6MezI62KDWambW21ODyz
# zkQmcOOhFKk+zfZWS10eVEiSHzT9R7wBLM4QUR7uq4wyjIFCkIH6cO+lp0Qaj8rp
# 0ZmPhePN56aKHI/pZyBxHVaheVC2L9IxtYyqNw+LV9HvukO/77UrPPNYDj0INVRr
# qeGRWHYUzPHAOk/lxjXaBcyLra0f0ZaOu7D+KZhDxLCVsZzajoBTVz6y7LY6QZtf
# 7VbwtFoWm9pZ9meavgoaP1RpvpICkbbTrTMvLGsCAwEAAaOBxjCBwzAPBgNVHRME
# CDAGAQH/AgEAMB0GA1UdDgQWBBTSM/OC2jIG+xFR2Vqmuz4UpnvH8TAfBgNVHSME
# GDAWgBSeZQWxjtBBwt7/y2umoUjA8jApPjALBgNVHQ8EBAMCAYYwNgYDVR0fBC8w
# LTAroCmgJ4YlaHR0cDovL2NybC5yYWNrc3BhY2UuY29tL1JhY2tDQUwxLmNybDAQ
# BgkrBgEEAYI3FQEEAwIBADAZBgkrBgEEAYI3FAIEDB4KAFMAdQBiAEMAQTANBgkq
# hkiG9w0BAQsFAAOCAgEARd7Is0LXxGE5t5NxYCeLVGNhyPDsk+N5V0h8Lz9A7xp9
# KKW2gNtuLIMkAd9pV9+cOmUhoTobAh40Nqh4rwSzKfz5+hzbPxHDg29HEAsZglqi
# QJSsVnUaJmAiQMDxmZ86tN8OHGMt7kxdgz0MDZMwoz/NmiLBCtL4IYIAY/GISzVF
# 1BKPpzdJjlEzO0KDu3zE0zkkPFnMxev/H+gwmUdGv0hAGxpwOkAWtTjaWR7YoX3f
# zSENnQJjSJPxMykZrmeIqM+KM/hBYAUhbjmcXqpYiNlXpkv/1LB/C9bQNSZ20ydz
# O2AsnDdd9CqIjSqDpji0TO8BLt1oia6Km29lG1BF8HEHy4IjJci7YwUQhak3VjkU
# UAnPDxRqjV+/6KJtW2dw7BtAVXDr2A7Q99oJ51KSV07wnmLbiDnH20mjFKe4ljW7
# DtVMXDjhoAJ4IN1OPd9QH4P09I1AVgapPXb9mbb26S7Y9p7tGV3iFN6Q+zPuV7AE
# Ci/e+gDTEPIRadO16T8RfBPhWkkT3IhSjBcnmIcPOM51z4x8yf5RZobNZNLfbIQj
# tMUSz+NxlgMUzhmXIUYE8+wPvIa/vobp494DXw7xc7LODKLlQI8VYZq6UguyhrjQ
# 9UWo4fMYGzsskhEQQuvSEli8HGmvImFkxi0HPELeB+NrQ4V8QhHSiKUraqlFZqUx
# ggFKMIIBRgIBATAqMBMxETAPBgNVBAMTCE9SRDFDQTAxAhMXAAAAuaWtmkZVgdp9
# AAAAAAC5MAkGBSsOAwIaBQCgeDAYBgorBgEEAYI3AgEMMQowCKACgAChAoAAMBkG
# CSqGSIb3DQEJAzEMBgorBgEEAYI3AgEEMBwGCisGAQQBgjcCAQsxDjAMBgorBgEE
# AYI3AgEVMCMGCSqGSIb3DQEJBDEWBBRPZbGNWNSY2Q8+QixYSDpnJ94jdTANBgkq
# hkiG9w0BAQEFAASBgHlUHkq0XVt5NrzikHTMNICOqjZt+tVTduvjbp+TUQmmGfY8
# jlSkvz8UHQ8+S/c+gotgb2JMRE5P93YUOvingeeh0urpTkKI3zNvTScNZl1eA6eM
# FpTOiEHChaYIFWuz/r9qWAvEgypZcusihKGY0zru0X1TcHNUvYBGxQmWjRxq
# SIG # End signature block
