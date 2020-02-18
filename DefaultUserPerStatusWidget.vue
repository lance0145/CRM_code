<template>
    <form id="sdaform" @submit.prevent="submit">
        <label>Default Agent per Status</label>
        <div class="col-sm-12 col-md-12 col-lg-12">
            <div class="form-group">
                <div class="row">
                    <div class="col-sm-12 col-md-6 col-lg-6">
                        <label for="status">Select Status</label>
                        <select name="Status" id="Status" class="form-control" v-model="formData.status" required>
                            <option value="">Please Select</option>
                            <option v-for="status in statuses" :value="status.Name">{{status.Name}}</option>
                        </select>
                    </div>

                    <div class="col-sm-12 col-md-6 col-lg-6">
                        <label for="isLead">Select Lead or Client?</label>
                        <select id="isLead" name="isLead" class="form-control" v-model="formData.isLead" required>
                            <option value="">Please Select</option>
                            <option value="true">Leads</option>
                            <option value="false">Clients</option>
                        </select>
                    </div>
                </div>

                <div class="row">
                    <div v-if="formData.isLead == 'false'" class="col-sm-12 col-md-6 col-lg-6">
                        <label for="uid">Select Retention Agent</label>
                        <select name="uid" id="uid" class="form-control" v-model="formData.agentid" required>
                            <option value="">Please Select</option>
                            <optgroup v-for="(group, type) in users" :label="type">
                                <option v-for="user in group" :value="user.username">{{user.username}}</option>
                            </optgroup>
                        </select>
                    </div>

                    <div v-else class="col-sm-12 col-md-6 col-lg-6">
                        <label for="uid">Select Sales Agent</label>
                        <select name="uid" id="uid" class="form-control" v-model="formData.agentid" required>
                            <option value="">Please Select</option>
                            <optgroup v-for="(group, type) in users" :label="type">
                                <option v-for="user in group" :value="user.username">{{user.username}}</option>
                            </optgroup>
                        </select>
                    </div>

                    <div class="col-sm-12 col-md-1 col-lg-12">
                        <button type="submit" id="addDefault" class="btn btn-primary pull-right" @click="createStatusDefaultAgent">Add Default <i class="fa fa-download"></i></button>
                        <button type="submit" id="editDefault" class="btn btn-primary pull-right" style="display : none" @click="updateStatusDefaultAgent">Edit Default <i class="fa fa-download"></i></button>
                    </div>
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-md-12 table-responsive" id="tableContainer">
                <table class="table table-hover table-no-wrap table-bordered table-striped">
                    <thead>
                        <tr class="table-row-header" role="row">
                            <th>Status</th>
                            <th>Context</th>
                            <th>Agent</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="d in defaults">
                            <td>{{d.Status}}</td>
                            <td>{{d.Context}}</td>
                            <td>{{d.AgentId}}</td>
                            <td>
                                <i class='fa fa-edit btn btn-success pull-right' @click='fetchStatusDefaultAgent(d.Id, d.Status, d.Context, d.AgentId)'></i>
                                <i class='fa fa-trash btn btn-danger pull-right' @click='deleteStatusDefaultAgent(d.Id)'></i>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </form>
</template>


<script>

    import axios from 'axios';

    const axiosHeaderConfig = {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
    };

    export default {
        props: {
            poolid: {
                required: true,
                type: String
            }
        },
        data: function () {
            return {
                statuses: [],
                defaults: [],
                users: [],
                formData: {
                    SDid: '',
                    agentid: '',
                    status: '',
                    isLead: '',
                },
            };
        },
        methods: {
            fetchStatus: function () {
                let endpoint = "/Webservice/Admin/StatusService.asmx/GetStatuses";
                axios.post(endpoint, {}, axiosHeaderConfig).then((response) => {
                    if (response.data.d)
                        this.statuses = response.data.d;
                }).catch((error) => {
                    toastr.error(error);
                });
            },
            fetchUsers: function () {
                var context = "";
                if (this.formData.isLead == 'true') {
                    context = "leads";
                }
                else {
                    context = "clients";
                }
                let endpoint = "/Webservice/Admin/UserService.asmx/GetUsersGroupByTypes";
                axios.post(endpoint, { context: context }, axiosHeaderConfig).then((response) => {
                    if (response.data.d)
                        this.users = response.data.d;
                }).catch((error) => {
                    toastr.error(error);
                });
            },
            fetchStatusDefaultAgent: function (id, status, context, agentid) {
                console.log(agentid);
                this.formData.SDid = id;
                $("#Status").focus();
                $("#Status").val(status);
                if (context == 'leads') {
                    $("#isLead").val("true");
                }
                else {
                    $("#isLead").val("false");
                }
                $("#uid").val(agentid);
                $("#addDefault").hide();
                $("#editDefault").show();
            },
            updateStatusDefaultAgent: function () {
                var self = this;
                if (self.formData.status !== "" && self.formData.agentid !== "" && self.formData.isLead !== "") {
                    if (confirm('Edit this Status Default Agent?')) {
                        var context = "";
                        if (this.formData.isLead == 'true') {
                            context = "leads";
                        }
                        else {
                            context = "clients";
                        }
                        console.log(self.formData.SDid);
                        console.log(self.formData.status);
                        console.log(self.formData.agentid);
                        console.log(context);
                        $.ajax({
                            url: '/Admin/Pools/Edit.aspx/UpdateStatusDefaultAgent',
                            type: 'POST',
                            contentType: 'application/json; charset = utf-8',
                            dataType: "json",
                            data: "{'id':'" + self.formData.SDid + "','status':'" + self.formData.status + "','agent':'" + self.formData.agentid + "','context':'" + context + "'}",
                            success: function (data) {
                                toastr.success("Status Default Agent Successfully Edit!");
                                self.readStatusDefaultAgent();
                            },
                            error: function (data) {
                                toastr.error("An error occured while processing your request.");
                            }
                        });
                    }
                    $("#addDefault").show();
                    $("#editDefault").hide();
                    self.formData.status = "";
                    self.formData.isLead = "";
                    self.formData.agentid = "";
                    $('#Status').prop('selectedIndex', 0);
                    $('#isLead').prop('selectedIndex', 0);
                    $('#uid').prop('selectedIndex', 0);
                }                
            },
            createStatusDefaultAgent: function () {
                var self = this;
                if (self.formData.status !== "" && self.formData.agentid !== "" && self.formData.isLead !== "") {
                    if (confirm('Add this Status Default Agent?')) {
                        var context = "";
                        if (this.formData.isLead == 'true') {
                            context = "leads";
                        }
                        else {
                            context = "clients";
                        }
                        $.ajax({
                            url: '/Admin/Pools/Edit.aspx/CreateStatusDefaultAgent',
                            type: 'POST',
                            contentType: 'application/json; charset = utf-8',
                            dataType: "json",
                            data: "{'id':'" + self.poolid + "','status':'" + self.formData.status + "','agent':'" + self.formData.agentid + "','context':'" + context + "'}",
                            success: function (data) {
                                toastr.success("Status Default Agent Successfully Add!");
                                self.readStatusDefaultAgent();
                            },
                            error: function (data) {
                                toastr.error("An error occured while processing your request.");
                            }
                        });
                    }
                    self.formData.status = "";
                    self.formData.isLead = "";
                    self.formData.agentid = "";
                    $('#Status').prop('selectedIndex', 0);
                    $('#isLead').prop('selectedIndex', 0);
                    $('#uid').prop('selectedIndex', 0);
                }
            },
            readStatusDefaultAgent: function () {
                var id = this.poolid;
                let endpoint = "/Admin/Pools/Edit.aspx/ReadStatusDefaultAgent";
                axios.post(endpoint, { id: id }, axiosHeaderConfig).then((response) => {
                    if (response.data.d)
                        this.defaults = response.data.d;
                }).catch((error) => {
                    toastr.error(error);
                });
            },
            deleteStatusDefaultAgent: function (id) {
                if (confirm('Delete this Status Default Agent?')) {
                    let endpoint = "/Admin/Pools/Edit.aspx/DeleteStatusDefaultAgent";
                    axios.post(endpoint, { id: id }, axiosHeaderConfig).then((response) => {
                        if (response.data.d)
                            toastr.success("Status Default Agent Successfully Deleted!");
                        this.readStatusDefaultAgent();
                    }).catch((error) => {
                        toastr.error(error);
                    });
                }
            },
        },
        watch: {
            'formData.isLead': function () {
                this.fetchUsers();
            }
        },
        mounted: function () {
            this.fetchStatus();
            this.readStatusDefaultAgent();
        },
    }
</script>