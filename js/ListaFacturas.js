
/* ************ GlFactura - Lista de facturas  --
    ListaFacturas.js


*/

// #region listaFactura  ----------------------------------------------------------   


function actListafacturas() {
    // Cuenta factura y crea el pager de forma asincrona --
    //var js = cuentaListaFacturas(); 
    setPageSize();
    // Parámetros de seleccion, usa variable global 
    paramSelListaFacturas = {
        nPagina: 0,
        pageSize: 0, 
        Orden: ordenListaFacturas,
        fDesde: $('#fFiltroDesde').val(),
        fHasta: $('#fFiltroHasta').val(),
        Ejercicio: $('#selEjercicio').val(),
        Cadena: $('#selCadenaFiltro').val(),
        Factura: $('#txSelFactura').val(),
        Cliente: $('#txSelIdCliente').val(),
        Pedido: $('#txSelPedido').val(),
        Albaran: $('#txSelAlbaran').val(),
        idSap: $('#txSelidSap').val(),
        HRuta: $('#txSelHRuta').val(),
        Facturados: $('input:radio[name=rbFac]:checked').val(),
        CargoAbono: $('input:radio[name=rbCA]:checked').val()       
    };
    //console.log('actListafacturas:' + JSON.stringify(paramSelListaFacturas));
    cuentaListaFacturas();
    cargaListaFacturas(1); 
};// actListafacturas --

function cuentaListaFacturas() {
    //console.log('cuentaListaFacturas:'+arguments.callee.name + 'INIC.' + minSecActual());
    //$body.addClass("loading");
    var js; // Parametro JSON a retornar --        
    //  Recupera solo nº de registros y totales. Con parametro nº de pagina 0, indica que solo se cuentan facturas --

    $.ajax({
        type: "POST", contentType: "application/json; charset=utf-8", dataType: "json", async: true,
        url: "wsGlFactura.asmx/recListaFacturasJSON",
        data: JSON.stringify(paramSelListaFacturas),
        success:
            function (data) {
                js = JSON.parse(data.d);    
                crearPagerListaFacturas(Math.ceil(js.NReg / PageSize));  // la funcion ceil devuelve 1 +  si hay decimales --
                $("#lbnReg").html(formatNum(js.NReg) + ' lineas');
                $("#lbTotal").html('Tot. ' + formatNum(js.Total.toFixed(0)));
                $("#lbCostes").html('Costes. ' + formatNum(js.TotCostes.toFixed(0)));
                // aqui, no se pone msg. de fin de actualizacíón  
                //console.log('cuentaListaFacturas '+arguments.callee.name + ' FIN.' + minSecActual());                
                //$body.removeClass("loading");
            },
        error:  function (request, status, error) { alert(JSON.parse(request.responseText).Message); }
    }); // $.ajax({    
    return js; // retorna parametro JSON completo --
} // cuentaListaFacturas --


function cargaListaFacturas(pagina) {
    //console.log(cargaListaFacturas');
    /* ref; uso de arguments 
    console.log(arguments.callee.name + ' INIC. pagina:' + pagina + ', pageSize:' + PageSize +
        ',fDesde:' + $('#fFiltroDesde').val() + 
        ',fHasta:' + $('#fFiltroHasta').val() + 
        ',Ejercicio:' + $('#selEjercicio').val() +
        minSecActual());*/
    $body.addClass("loading");
    var textOrden = '';
    // si se estan recupernado registros, se indican los parametros nPagina y pageSize --
    paramSelListaFacturas.nPagina = pagina;
    paramSelListaFacturas.pageSize = PageSize;
    $.ajax({
        type: "POST", contentType: "application/json; charset=utf-8", dataType: "json", async: true,
        url: "wsGlFactura.asmx/recListaFacturasJSON",
        data: JSON.stringify(paramSelListaFacturas),       
        success:
            function (data) {
                if ((data.d.length < 30) && (data.d.substring(0, 6) == 'limite'))
                    msgAler('Se han Generado demasiadas lineas, mas de ' + data.d)
                else {
                    var js = JSON.parse(data.d);
                    // Crea grid con detalles recuperados en json --                                    
                    creaGridListaFacturas(js, '#gvListaFacturas');
                }
                //console.log('cargaListaFacturas '+arguments.callee.name + ' FIN.' + minSecActual());
                $body.removeClass("loading");
            },
        error: function (request, status, error) { alert(JSON.parse(request.responseText).Message); }
    }); // $.ajax({

} // cargaListaFacturas --

function creaGridListaFacturas(det, grid) {
    //alert('creaGridListaFacturas');
    var htmCab = ' <thead class="cabecera-fija"> <tr >  ' +
         '<th style="display:none"> Fila (OCULTA)</th> ' +
         '<th style="width:25px"> Sel </th> ' +
         '<th style="width:50px"> Fact.</th> ' +

         '<th style="width:40px"> Ejer.</th> ' +
         '<th style="width:72px"> Fecha</th> ' +
         '<th style="width:50px"> Cliente</th> ' +
         '<th style="width:75px"> CSAP </th> ' +
         '<th style="width:260px"> Nombre Cliente</th> ' +
         '<th style="width:70px"> Total</th> ' +
         '<th style="width:70px"> Coste</th> ' +
         '<th style="width:30px"> Lin </th> ' +
         '<th style="width:50px"> Pedido </th> ' +
         '<th style="width:50px"> Albaran </th> ' +
         '<th style="width:100px"> H. Ruta </th> ' +
         '<th style="width:115px"> Referencia </th> ' +
         '<th style="width:70px"> F.Cont </th> ' +
         '<th style="width:180px"> Cadena</th> ' +
         '</tr> </thead> <tbody></tbody>';
    $(grid).html(htmCab);
    // Recorre datos de detalles --
    var iEfectivo;
    
    $.each(det, function (i, item) {        
        // ref: https://stackoverflow.com/questions/5430254/jquery-selecting-table-rows-with-checkbox
        var htmLinDet = '<tr>' +
          '<td style="display:none">' + item.Fila + '</td>' +
          '<td style="width:25px;"> <input id="ckSellin"  type="checkbox"  class="ckb custom-control custom-checkbox" value="' + item.idFactura + '" /> </td>' +
          '<td id="idFactura" style="width:50px;">' + item.idFactura + '</td>' +
          '<td id="Ejercicio" style="width:40px;">' + item.Ejercicio + '</td>' +
          '<td style="width:72px;">' + item.Fecha + '</td>' +
          '<td style="width:50px;">' + item.idCliente + '</td>' +
          '<td style="width:75px;">' + item.idSap + '</td>' +
          '<td style="width:260px;">' + item.NombreCliente + '</td>' +
          '<td style="width:70px; text-align:right">' + formatNum(item.Total.toFixed(2)) + '</td>' +
          '<td style="width:70px; text-align:right">' + formatNum(item.Coste.toFixed(2)) + '</td>' +
          '<td style="width:30px">' + item.NReg + '</td>' +
          '<td style="width:50px">' + item.Pedido + '</td>' +
          '<td style="width:50px">' + item.Albaran + '</td>' +
          '<td style="width:100px">' + item.HRuta + '</td>' +
          '<td style="width:115px">' + item.Referencia + '</td>' +
          '<td id="FContabilizada" style="width:70px">' + item.FContabilizada + '</td>' +
          '<td style="width:180px;">' + item.NombreCadena + '</td>' +
          '</tr>';
        $(grid + ' tbody').append(htmLinDet);
    });// each --

    
    eventosGridListaFacturas(grid);

   
}//creaGridListaFacturas --


function eventosGridListaFacturas(grid) {
    // al  seleccionar el check de una linea del grid, usa clase "ckb" --    
    $(grid + " .ckb").change(function (e) {
        if ($(this).is(":checked")) {
            $(this).closest('tr').addClass("selFila");
        } else {
            $(this).closest('tr').removeClass("selFila");
            //alert('DESMarcada');
        }
    });

    // ref: añade eventos al grid, (solo body, no incluye thead ni tfoot)  --
    // Para cada celda --    
    $(grid + ' tbody td ').mouseover(function (e) {
        // Cambia color de celda seleccionada, usa clase CSS  --
        $(grid + ' tbody td').removeClass('selCelda'); // elimina antes clase de TODO el body --
        $(this).addClass('selCelda');

        e.stopPropagation();
    });


    // Para cada Fila,         
    $(grid + ' tbody tr ').click(function (e) {
        var fila = $(this);
        var filaInd = fila.parent().children().index($(this)); // indice de la fila --
        // ref: NO borrar,  //  accede a columna especifica de fila por su nº de col --                
        //  ref: var celSel = $(grid + ' tbody tr:eq(' + filaInd + ') > td:eq(5)');   //  accede a columna especifica de fila por su nº de col --
        var celSel = fila.find('#idFactura'); // accede a columna especifica de fila por su id --            

        //fila.css('background-color', 'beige');  //cambia color solo de la linea seleccionada  --                 
        //abrirFrmFactura(celSel.html());

        e.stopPropagation();
    });

    $(grid + ' tbody tr ').dblclick(function (e) {
        //var fila = $(this);            
        var celSel = $(this).find('#idFactura'); // accede a columna especifica de fila por su id --                        
        abrirFrmFactura(celSel.html());

        e.stopPropagation();
    });

    // Recorre todas las filas del grid--        
    $(grid + ' tbody tr ').each(function () {
        var fila = $(this);
        var celSel = fila.find('#FContabilizada'); // accede a columna especifica de fila por su id --
        if (celSel.html() != "") {
            celSel.toggleClass("linContabilizada"); // cambia color del campo factura                           
        }
    });


    // si no hay orden definido, 0 usa el del factura, col 2
    if (ordenListaFacturas == 0)
        ordenListaFacturas = 3;

    // ref: indica columna ordenada, añade clase css a una columna de la tabla de titulos, ojo hay columnas ocultas
    // Los nombres debene de coincidir con titulo 
    var cel = $(grid + ' tr:eq(0) th:nth-child(' + ordenListaFacturas + ')');
    cel.addClass('cabOrdenadaDesc');

    var camposOrden = ['Fact.', 'Fecha', 'Cliente','Cadena'];

    // recorre columnas de la cabecera para asignar evento de orden y cambiar formato de la cab 
    $(grid + ' th').each(function () {    
        //$('#gvListaFacturas').find('thead th').each(function () {
        var celda = $(this);
        var titulo = celda.html().trim();
        var idTh = celda.index();
        // si es un campo de la lista de ordenables y la columna no esta ya ordenada 
        if (camposOrden.indexOf(titulo) >= 0 && celda.attr("class") != 'cabOrdenadaDesc') {
            celda.addClass('cabOrdenable');
            celda.click(function (e) {
                ordenListaFacturas = idTh + 1;
                //alert('ordenListaFacturas:' + ordenListaFacturas);
                actListafacturas();
            });           
        };
    });// each --
    
   
    // Referencias ----------------------   
    // ref: para recorrer cabecera de las filas del grid
    //$('#gvListaFacturas th').each(function () {
    //    var cell = $(this);
    //    //alert('Fila:'+cell.index() + 'Contiene:'+cell.html());
    //});
    // acceso directo a una celda de la cabecera (th) --
    //alert('Fila 3:' + $('#gvListaFacturas').find("th:eq(3)").html());


};// eventosGridListaFacturas --

function crearPagerListaFacturas(Paginas) {
    // usa componente bootpag ref: http://botmonster.com/jquery-bootpag/#docs     
    //console.log(arguments.callee.name + 'pag:'+Paginas+minSecActual());    
    $('#pager1').unbind('page'); //ELIMINAR antes el evento 'page' para que no se ejecute más veces --
    $('#pager1').bootpag({
        total: Paginas,
        page: 1,
        maxVisible: 5,
        leaps: true,
        firstLastUse: true,
        first: '←', last: '( → ' + formatNum(Paginas.toFixed(0)) + ')'
        //wrapClass: 'pagination', activeClass: 'active', disabledClass: 'disabled', nextClass: 'next', prevClass: 'prev',lastClass: 'last',  firstClass: 'first',
    }).on("page", function (event, num) {
        //$(".content4").html("Page " + num); 
        //alert('EJECUTADO on page, ');         
        cargaListaFacturas(num);
    });
}// crearPagerListaFacturas --


function ListaFacturasExcel() {
    $.ajax({
        type: "POST", async: false, contentType: "application/json; charset=utf-8",
        url: "wsGlFactura.asmx/excelListaFacturas",
        data: '{fDesde:"' + $('#fFiltroDesde').val() + '"' +
              ',fHasta:"' + $('#fFiltroHasta').val() + '"' +
              ',Ejercicio:' + $('#selEjercicio').val() +
              ',Cadena:' + $('#selCadenaFiltro').val() +
              ',Factura:"' + $('#txSelFactura').val() + '"' +
              ',Cliente:"' + $('#txSelIdCliente').val() + '"' +
               ',Pedido:"' + $('#txSelPedido').val() + '"' +
              ',Albaran:"' + $('#txSelAlbaran').val() + '"' +
              ',idSap:"' + $('#txSelidSap').val() + '"' +
              ',HRuta:"' + $('#txSelHRuta').val() + '"' +
              ',Facturados:"' + $('input:radio[name=rbFac]:checked').val() + '"' +
              ',CargoAbono:"' + $('input:radio[name=rbCA]:checked').val() + '"' +
         '}',
        dataType: "json",
        success: function (data) {
            // abre el fichero en el explorador --
            window.open('Informes/listaFacturas.xls', '_blank');
        },
        error: function (request, status, error) { alert(JSON.parse(request.responseText).Message); }
    }); // $.ajax({

    // abre el fichero en el explorador --
    //window.open('Informes/listaFacturas.xls', '_blank');
}// ListaFacturasExcel --

function selFechasPred() {
    //alert('selFechasPred: Valor ' + $('input:radio[name=rbSelF]:checked').val());
    selFp = $('input:radio[name=rbSelF]:checked').val();

    var d = new Date();
    var dayOfMonth = d.getDate();
    var FechaActual = d.getDate() + "/" + (d.getMonth() + 1) + "/" + d.getFullYear(); // fecha actual (?? por algún motivo coje un mes menos )---  

    $('#fFiltroHasta').val(FechaActual);
    switch (selFp) {
        case "0":
            $('#fFiltroDesde').val("01/01/1900");
            $('#fFiltroHasta').val('01/01/2100');
            break;
        case "1": //  2 días --                
            d.setDate(dayOfMonth - 2); // resta dias de la fecha actual --
            $("#fFiltroDesde").val(d.getDate() + "/" + (d.getMonth() + 1) + "/" + d.getFullYear());
            break;
        case "2": // Una semana                
            d.setDate(dayOfMonth - 7);
            $("#fFiltroDesde").val(d.getDate() + "/" + (d.getMonth() + 1) + "/" + d.getFullYear());
            break;
        case "3": //  Un mes --            
            d.setDate(dayOfMonth - 30);
            $("#fFiltroDesde").val(d.getDate() + "/" + (d.getMonth() + 1) + "/" + d.getFullYear());
            break;
        case "4":  //  año actual, desde el día 1            
            $("#fFiltroDesde").val("01/01/" + d.getFullYear());
            break;
    };
}// selFechasPred --

function fncapacapaFiltroFacturas(capa, x, y) {

    if ($(capa).is(":visible")) // si ya esta visible, no hace nada --
        return;  // aqui, mal genera error 

    $('.capa').hide(); // oculta TODAS capas --   
    $(capa).css('left', 10);
    $(capa).css('top', y + 8);
    $(capa).show();
}// fncapacapaFiltroFacturas --

function setPageSize() {
    PageSize = $('#selLinPag').val();
}// setPageSize --

// #endregion listaFactura  ------------------

// OBSOLETOS --







