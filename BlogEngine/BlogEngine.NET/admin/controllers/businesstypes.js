angular.module('blogAdmin').controller('BusinessTypesController', ["$rootScope", "$scope", "$location", "$http", "$filter", "dataService", function ($rootScope, $scope, $location, $http, $filter, dataService) {
    $scope.items = [];
    $scope.lookups = [];
    $scope.businesstype = {};
    $scope.id = ($location.search()).id;
    $scope.focusInput = false;

    if ($scope.id) {
        dataService.getItems('/api/businesstypes', { Id: $scope.id })
            .success(function (data) { angular.copy(data, $scope.businesstype); })
            .error(function () { toastr.error("Error loading Business Type"); });
        $("#modal-add-biz").modal();
        $scope.focusInput = true;
    }

    $scope.addNew = function () {
        $scope.clear();
        $("#modal-add-biz").modal();
        $scope.focusInput = true;
    }

    $scope.load = function () {
        dataService.getItems('/api/lookups')
            .success(function (data) { angular.copy(data, $scope.lookups); })
            .error(function () { toastr.error("Error loading lookups"); });

        var p = { skip: 0, take: 0 };
        dataService.getItems('/api/businesstypes', p)
            .success(function (data) {
                angular.copy(data, $scope.items);
                gridInit($scope, $filter);
                rowSpinOff($scope.items);
            })
            .error(function () {
                toastr.error("Error loading Business Types");
            });
    }

    $scope.load();

    $scope.save = function () {
        if (!$('#form').valid()) {
            return false;
        }
        if ($scope.businesstype.Id) {
            dataService.updateItem("/api/businesstypes/update/" + $scope.businesstype.Id, $scope.businesstype)
           .success(function (data) {
               toastr.success("Business type updated");
               $scope.load();
           })
           .error(function (data) { toastr.error(data); });
        }
        else {
            dataService.addItem("/api/businesstypes", $scope.businesstype)
           .success(function (data) {
               toastr.success("Business type added");
               if (data.Id) {
                   angular.copy(data, $scope.businesstype);
                   $scope.load();
               }
           })
           .error(function (data) { toastr.error(data); });
        }
        $("#modal-add-biz").modal('hide');
        $scope.focusInput = false;
    }

    $scope.processChecked = function (action) {
        processChecked("/api/businesstypes/processchecked/", action, $scope, dataService);
    }

    $scope.clear = function () {
        $scope.businesstype = { "IsChecked": false, "Id": null, "BusinessType": "", "Count": 0 };
        $scope.id = null;
    }

    $(document).ready(function () {
        $('#form').validate({
            rules: {
                txtSlug: { required: true }
            }
        });
    });
}]);