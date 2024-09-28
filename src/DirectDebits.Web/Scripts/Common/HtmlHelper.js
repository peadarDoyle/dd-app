htmlHelper.makeUnorderedList = function (array) {
    let list = document.createElement('ul');

    for (let i = 0; i < array.length; i++) {
        let item = document.createElement('li');
        item.appendChild(document.createTextNode(array[i]));
        list.appendChild(item);
    }

    return list;
};

dateHelper.formatddMMyyyy = function (date) {
    let day = date.getDate().toString();
    day = day.length > 1 ? day : '0' + day;

    let month = (date.getMonth() + 1).toString();;
    month = month.length > 1 ? month : '0' + month;

    let year = date.getFullYear();

    return day + '-' + month + '-' + year;
}