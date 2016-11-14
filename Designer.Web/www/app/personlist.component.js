"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var core_1 = require('@angular/core');
var http_1 = require('@angular/http');
var PersonListComponent = (function () {
    function PersonListComponent(http) {
        this.http = http;
        this.loadPersons();
    }
    PersonListComponent.prototype.loadPersons = function () {
        var _this = this;
        this.http.get("http://localhost:9000/api/person/list")
            .subscribe(function (r) { return _this.persons = r.json(); });
    };
    PersonListComponent = __decorate([
        core_1.Component({
            templateUrl: 'templates/personlist.html',
        }), 
        __metadata('design:paramtypes', [http_1.Http])
    ], PersonListComponent);
    return PersonListComponent;
}());
exports.PersonListComponent = PersonListComponent;
//# sourceMappingURL=personlist.component.js.map