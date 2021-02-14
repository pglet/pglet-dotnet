Remove-Module pglet -ErrorAction SilentlyContinue
Import-Module ([IO.Path]::Combine((get-item $PSScriptRoot).parent.FullName, 'pglet.psd1'))

Connect-PgletPage -Name "index" -NoWindow

Invoke-Pglet "clean page"

Invoke-Pglet "add
text value='Vertical chart with textual x axis' size=xlarge
chart type=VerticalBar tooltips legend=false width=300 height=300 yticks=5 barWidth=20 yFormat='{y}%'
  data
    p id=rdp x=Red y=20 color=Crimson legend=Red ytooltip='20%'
    p x=Green y=50 color=ForestGreen legend=Green ytooltip='50%'
    p x=Blue y=30 color=blue legend=Blue ytooltip='30%'

text value='Vertical chart with numeric x axis' size=xlarge
chart type=VerticalBar tooltips legend=true width='100%' height=400 xnum ymax=50 yticks=5 barWidth=20 yFormat='$,' colors='green salmon'
  data id=d1
    p x=1 y=5a legend=A ytooltip='5%' color=blue
    p x=2 y=10cs legend=B ytooltip='10%'
    p x=3 y=7cc legend=C ytooltip='7%'
    p x=4 y=a3c0 legend=D
    p x=5 y=1c legend=E
  data
    p x=4 y=30
    p x=5 y=1    
"

for($i = 20; $i -lt 41; $i++) {
  Invoke-Pglet "setf rdp y=$i"
  Start-Sleep -ms 100
}

Start-Sleep -s 2
Invoke-Pglet "add to=d1
  p x=6 y=20"
Invoke-Pglet "remove d1 at=0"

Start-Sleep -s 2
Invoke-Pglet "add to=d1
  p x=7 y=10"
Invoke-Pglet "remove d1 at=0"