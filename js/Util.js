

// ************ Utilidades -------------------



// #region fechas --

// recibe fecha en formato texto formato 'dd/mm/yyy' o  'dd/mm/yyy hh:mm', retorna tipo date. Act. 19/7/18. Para validar fecha y hora conjuntas-- --
function textFecha(Texto) {
    // divide fecha recibido por caracter '/' 
    var dateParts = Texto.split("/");
    // ref: da la vuelta al  mes/dia, parse funciona con mm/dd/yyy , no con dd/mm/yyy,  --
    var fecha2 = dateParts[1] + '/' + dateParts[0] + '/' + dateParts[2];
    var d = new Date(Date.parse(fecha2));
    //console.log('textFecha resul:'+d);
    // retorna texto 'Invalid Date'  si la conversión no es correcta --
    return d;
};// textFecha


// cambia formato de fecha a dd/mm/yyyy, entrada objeto tipo date, retorna string 
function fechaddmm(d) {
    return d.getDate() + "/" + (d.getMonth() + 1) + "/" + d.getFullYear();
};

// Retorna fecha del primer día del mes de una fecha de entrada
function primerDiaMes(d) {
    return new Date(d.getFullYear(), d.getMonth(), 1);
};

// Retorna fecha del ultimo día del mes de una fecha de entrada
function ultimoDiaMes(d) {
    return new Date(d.getFullYear(), d.getMonth() + 1, 0);
};


// cambia formato de fecha a dd/mm/yyyy hh:mm, entrada objeto tipo date, retorna string 
function fechaddmm_hh(d) {
    return d.getDate() + "/" + (d.getMonth() + 1) + "/" + d.getFullYear() + " " + d.getHours() + ":" + d.getMinutes();
};


// validaciones de fechas y hora ---

// valida solo fecha --
function fechaValida(fecha) {
    var pattern = /^(?:(?:0?[1-9]|1\d|2[0-8])(\/|-)(?:0?[1-9]|1[0-2]))(\/|-)(?:[1-9]\d\d\d|\d[1-9]\d\d|\d\d[1-9]\d|\d\d\d[1-9])$|^(?:(?:31(\/|-)(?:0?[13578]|1[02]))|(?:(?:29|30)(\/|-)(?:0?[1,3-9]|1[0-2])))(\/|-)(?:[1-9]\d\d\d|\d[1-9]\d\d|\d\d[1-9]\d|\d\d\d[1-9])$|^(29(\/|-)0?2)(\/|-)(?:(?:0[48]00|[13579][26]00|[2468][048]00)|(?:\d\d)?(?:0[48]|[2468][048]|[13579][26]))$/; // ok --

    return pattern.test(trim(fecha));
} // fechaValida --

function horaValida(hora) {
    //var pattern = /^[0-2][0-9]:[0-5][0-9]$/;  // 12 horas, requiere 2 dígitos en hora 
    //var pattern = /^(?:0?\d|1[012]):[0-5]\d$/ // 12 horas, NO requiere 2 dígitos en hora 

    //var pattern = /^(((([0-1][0-9])|(2[0-3])):?[0-5][0-9])|(24:?00))/ // 24 horas, requiere 2 dígitos en hora 
    //var pattern = /^(?:[01]\d|2[0-3]):?[0-5]\d$/ // 24 horas, requiere 2 dígitos en hora  ok, mas sencillo

    var pattern = /(^((2[0-3])|([01]?[0-9])):[0-5][0-9]$)|(^((1[0-2])|(0?[1-9])(:[0-5][0-9])?)[pa]m$)/;  // OK, 24 horas, NO requiere 2 dígitos

    return pattern.test(trim(hora));

} // horaValida --


function fechaValidaTexto(fecha) {
    // requiere Date.prototype.fechaVal -- 
    // alert('fechaValidaTexto:' + fecha)
    var d = new Date(fecha);
    if (!d.fechaVal()) {
        return false;
    }
    return true;
};// fechaValidaTexto --


Date.prototype.fechaVal = function () {
    return isFinite(this);
};


// genera array/tabla de rango entre dos fechas
var genFechas = function (startDate, endDate) {
    // ref: ejemplos llamada --
    //var dates = genFechas(new Date(2018, 1, 22), new Date(2018, 1, 25)); // 1 -> febrero 
    //var dates = genFechas(new Date(2018, 0, 22), new Date(2018, 1, 0)); // Desde 22 de enero hasta ultimo día de enero (
    //var dates = genFechas(new Date(2018, 1, 25), new Date(2018, 2, 0)); // Desde 25 de Feb hasta ultimo día de feb
    var dates = [],
        currentDate = startDate,
        addDays = function (days) {
            var date = new Date(this.valueOf());
            date.setDate(date.getDate() + days);
            return date;
        };
    while (currentDate <= endDate) {
        dates.push(currentDate)
        currentDate = addDays.call(currentDate, 1);
    }
    return dates;
};


// retorna minutos y segunos actuales para usarlo en debug 
function minSecActual() {
    return ' Min:' + (new Date()).getMinutes() + ', Seg:' + (new Date()).getSeconds();
}; // mi
// #endregion fechas --


function validarNIF(nif) {

    /*        
        Retorna: 
            False: Documento invalido.
            DNI: Correcto, se trata de un CIF/DNI
            NIE: Correcto, se trata de in NIE (extranjero)
            CIF: Correcto, se trata de in NIF (Empresa)

        Los DNI españoles pueden ser:
        NIF (Numero de Identificación Fiscal) - 8 numeros y una letra1
        NIE (Numero de Identificación de Extranjeros) - 1 letra2, 7 numeros y 1 letra1
        1 - Una de las siguientes: TRWAGMYFPDXBNJZSQVHLCKE
        2 - Una de las siguientes: XYZ           

        ref: https://github.com/TORR3S/Check-NIF/blob/master/checkNIF.js  
     */
    
    nif = nif.toUpperCase().replace(/[\s\-]+/g, '');
    if (/^(\d|[XYZ])\d{7}[A-Z]$/.test(nif)) {
        var num = nif.match(/\d+/);
        num = (nif[0] != 'Z' ? nif[0] != 'Y' ? 0 : 1 : 2) + num;
        if (nif[8] == 'TRWAGMYFPDXBNJZSQVHLCKE'[num % 23]) {
            return /^\d/.test(nif) ? 'DNI' : 'NIE';
        }
    }
    else if (/^[ABCDEFGHJKLMNPQRSUVW]\d{7}[\dA-J]$/.test(nif)) {
        for (var sum = 0, i = 1; i < 8; ++i) {
            var num = nif[i] << i % 2;
            var uni = num % 10;
            sum += (num - uni) / 10 + uni;
        }
        var c = (10 - sum % 10) % 10;
        if (nif[8] == c || nif[8] == 'JABCDEFGHI'[c]) {
            return /^[KLM]/.test(nif) ? 'ESP' : 'CIF';
        }
    }
    return false;
};// validarNIF



function soloLetrasyNum(text){
   //var alphanumeric = "someStringHere";
    let myRegEx = /[^a-z\d]/i;
    return isValid = !(myRegEx.test(text));
};// soloLetrasyNum 





function trim(cadena) {
    return cadena.replace(/^\s+/g, '').replace(/\s+$/g, '')
}// trim --

function esNumero(numero) {
    //n = numero.replace(".", "")// Elima antes los puntos        
    n = numero.replace(",", ".") // cambia comas por puntos --
    if (isNaN(n))
        return false;
    else
        return true;
}// esNumero --


// Elimina el formato para obtener datos enteros --
function intVal(i) {
    return typeof i === 'string' ? i.replace(/[\$,]/g, '') * 1 : typeof i === 'number' ? i : 0;
}; // intVal --

// Elimina el formato para obtener datos Float --
function floatVal(i) {
    return i.replace(/[\$.]/g, '');
}; // floatVal --

// Formatea nº añadiendo "." y ","  ref: http://stackoverflow.com/questions/1068284/format-numbers-in-javascript
function formatNum(str) {
    var parts = (str + "").split("."),
        main = parts[0],
        len = main.length,
        output = "",
        first = main.charAt(0),
        i;

    if (first === '-') {
        main = main.slice(1);
        len = main.length;
    } else {
        first = "";
    }
    i = len - 1;
    while (i >= 0) {
        output = main.charAt(i) + output;
        if ((len - i) % 3 === 0 && i > 0) {
            output = "." + output;
        }
        --i;
    }
    // put sign back --
    output = first + output;
    // put decimal part back --
    if (parts.length > 1) {
        output += "," + parts[1];
    }
    return output;
}// formatNum --


const BotonesMaxMin = function (PanelIzquierdo, PanelDerecho, Botones, AnchoTotal, PorcentajeAmpliado) {
    // trata evento de botones de maximizar,  minimizar, proporcional y dinamico
    //alert('BotonesMaxMin');
    $(Botones + ' i').click(function (e) {
        let Llamador = event.target;
        // Recuperar las clases del llamador
        let ClaseLlamador = $(Llamador).attr('class');
        let AnchoListaGrande = AnchoTotal * PorcentajeAmpliado / 100;

        PanelDerecho.unbind('click');
        PanelIzquierdo.unbind('click');
        if ($(Llamador).hasClass('fa-list-alt')) {
            //alert('Maximiza Lista (Izquierda)');            
            PanelIzquierdo.css('width', AnchoListaGrande);
            PanelDerecho.css('width', AnchoTotal - AnchoListaGrande);
        }
        if ($(Llamador).hasClass('fa-window-maximize')) {
            //alert('Maximiza Contenido (Derecha)');
            PanelDerecho.css('width', AnchoListaGrande);
            PanelIzquierdo.css('width', AnchoTotal - AnchoListaGrande);
        }
        if ($(Llamador).hasClass('fa-columns')) {
            //alert('Tamaño Proporcional 50%');

            let AnchoListaGrande = AnchoTotal * 50 / 100;

            PanelIzquierdo.css('width', AnchoListaGrande);
            PanelDerecho.css('width', AnchoTotal - AnchoListaGrande);
        }
        if ($(Llamador).hasClass('fa-window-restore')) {
            // al entrar o salir de Lista o edicion, cambia su tamaño. Total suma de los 2 paneles         
            //alert('Tamaño Dinamico');                        
            PanelIzquierdo.click(function (e) {
                /* pendiente: 6/10/20controlar si ya maximizado, sale. NO FUNCIONA,  outerWidth no coincide con el ancho                        
                    alert('MAXIMIZA Lista, outerWidth:' + PanelIzquierdo.outerWidth(true) + ', AnchoListaGrande:' + AnchoListaGrande);                                  
                    if (PanelIzquierdo.outerWidth(true) == AnchoListaGrande) {
                        alert('Ya maxim');              return;    }*/

                PanelIzquierdo.css('width', AnchoListaGrande);
                PanelDerecho.css('width', AnchoTotal - AnchoListaGrande);
            });

            PanelDerecho.click(function (e) {
                //alert('MAXIMIZA Edit');                
                PanelDerecho.css('width', AnchoListaGrande);
                PanelIzquierdo.css('width', AnchoTotal - AnchoListaGrande);

            });            
            PanelIzquierdo.trigger('click');
        }
    });

};// BotonesMaxMin





