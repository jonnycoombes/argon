﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace JCS.Argon.Services.Soap.Opentext
{
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="urn:Core.service.livelink.opentext.com", ConfigurationName="JCS.Argon.Services.Soap.Opentext.Authentication")]
    public interface Authentication
    {
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:Core.service.livelink.opentext.com/AuthenticateApplication", ReplyAction="urn:Core.service.livelink.opentext.com/Authentication/AuthenticateApplicationResp" +
            "onse")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.AuthenticateApplicationResponse> AuthenticateApplicationAsync(JCS.Argon.Services.Soap.Opentext.AuthenticateApplicationRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:Core.service.livelink.opentext.com/AuthenticateUser", ReplyAction="urn:Core.service.livelink.opentext.com/Authentication/AuthenticateUserResponse")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<string> AuthenticateUserAsync(string userName, string userPassword);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:Core.service.livelink.opentext.com/AuthenticateUserWithApplicationToken", ReplyAction="urn:Core.service.livelink.opentext.com/Authentication/AuthenticateUserWithApplica" +
            "tionTokenResponse")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<string> AuthenticateUserWithApplicationTokenAsync(string userName, string userPassword, string applicationToken);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:Core.service.livelink.opentext.com/CombineApplicationToken", ReplyAction="urn:Core.service.livelink.opentext.com/Authentication/CombineApplicationTokenResp" +
            "onse")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.CombineApplicationTokenResponse> CombineApplicationTokenAsync(JCS.Argon.Services.Soap.Opentext.CombineApplicationTokenRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:Core.service.livelink.opentext.com/GetOTDSResourceID", ReplyAction="urn:Core.service.livelink.opentext.com/Authentication/GetOTDSResourceIDResponse")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<string> GetOTDSResourceIDAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:Core.service.livelink.opentext.com/GetOTDSServer", ReplyAction="urn:Core.service.livelink.opentext.com/Authentication/GetOTDSServerResponse")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<string> GetOTDSServerAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:Core.service.livelink.opentext.com/GetSessionExpirationDate", ReplyAction="urn:Core.service.livelink.opentext.com/Authentication/GetSessionExpirationDateRes" +
            "ponse")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.GetSessionExpirationDateResponse> GetSessionExpirationDateAsync(JCS.Argon.Services.Soap.Opentext.GetSessionExpirationDateRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:Core.service.livelink.opentext.com/ImpersonateApplication", ReplyAction="urn:Core.service.livelink.opentext.com/Authentication/ImpersonateApplicationRespo" +
            "nse")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.ImpersonateApplicationResponse> ImpersonateApplicationAsync(JCS.Argon.Services.Soap.Opentext.ImpersonateApplicationRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:Core.service.livelink.opentext.com/ImpersonateUser", ReplyAction="urn:Core.service.livelink.opentext.com/Authentication/ImpersonateUserResponse")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.ImpersonateUserResponse> ImpersonateUserAsync(JCS.Argon.Services.Soap.Opentext.ImpersonateUserRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:Core.service.livelink.opentext.com/RefreshToken", ReplyAction="urn:Core.service.livelink.opentext.com/Authentication/RefreshTokenResponse")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.RefreshTokenResponse> RefreshTokenAsync(JCS.Argon.Services.Soap.Opentext.RefreshTokenRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="urn:Core.service.livelink.opentext.com/ValidateUser", ReplyAction="urn:Core.service.livelink.opentext.com/Authentication/ValidateUserResponse")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<string> ValidateUserAsync(string capToken);
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:api.ecm.opentext.com")]
    public partial class OTAuthentication
    {
        
        private string authenticationTokenField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string AuthenticationToken
        {
            get
            {
                return this.authenticationTokenField;
            }
            set
            {
                this.authenticationTokenField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="AuthenticateApplication", WrapperNamespace="urn:Core.service.livelink.opentext.com", IsWrapped=true)]
    public partial class AuthenticateApplicationRequest
    {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="urn:api.ecm.opentext.com")]
        public JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:Core.service.livelink.opentext.com", Order=0)]
        public string applicationID;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:Core.service.livelink.opentext.com", Order=1)]
        public string password;
        
        public AuthenticateApplicationRequest()
        {
        }
        
        public AuthenticateApplicationRequest(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication, string applicationID, string password)
        {
            this.OTAuthentication = OTAuthentication;
            this.applicationID = applicationID;
            this.password = password;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="AuthenticateApplicationResponse", WrapperNamespace="urn:Core.service.livelink.opentext.com", IsWrapped=true)]
    public partial class AuthenticateApplicationResponse
    {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="urn:api.ecm.opentext.com")]
        public JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:Core.service.livelink.opentext.com", Order=0)]
        public string AuthenticateApplicationResult;
        
        public AuthenticateApplicationResponse()
        {
        }
        
        public AuthenticateApplicationResponse(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication, string AuthenticateApplicationResult)
        {
            this.OTAuthentication = OTAuthentication;
            this.AuthenticateApplicationResult = AuthenticateApplicationResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="CombineApplicationToken", WrapperNamespace="urn:Core.service.livelink.opentext.com", IsWrapped=true)]
    public partial class CombineApplicationTokenRequest
    {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="urn:api.ecm.opentext.com")]
        public JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:Core.service.livelink.opentext.com", Order=0)]
        public string applicationToken;
        
        public CombineApplicationTokenRequest()
        {
        }
        
        public CombineApplicationTokenRequest(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication, string applicationToken)
        {
            this.OTAuthentication = OTAuthentication;
            this.applicationToken = applicationToken;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="CombineApplicationTokenResponse", WrapperNamespace="urn:Core.service.livelink.opentext.com", IsWrapped=true)]
    public partial class CombineApplicationTokenResponse
    {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="urn:api.ecm.opentext.com")]
        public JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:Core.service.livelink.opentext.com", Order=0)]
        public string CombineApplicationTokenResult;
        
        public CombineApplicationTokenResponse()
        {
        }
        
        public CombineApplicationTokenResponse(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication, string CombineApplicationTokenResult)
        {
            this.OTAuthentication = OTAuthentication;
            this.CombineApplicationTokenResult = CombineApplicationTokenResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetSessionExpirationDate", WrapperNamespace="urn:Core.service.livelink.opentext.com", IsWrapped=true)]
    public partial class GetSessionExpirationDateRequest
    {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="urn:api.ecm.opentext.com")]
        public JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication;
        
        public GetSessionExpirationDateRequest()
        {
        }
        
        public GetSessionExpirationDateRequest(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication)
        {
            this.OTAuthentication = OTAuthentication;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetSessionExpirationDateResponse", WrapperNamespace="urn:Core.service.livelink.opentext.com", IsWrapped=true)]
    public partial class GetSessionExpirationDateResponse
    {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="urn:api.ecm.opentext.com")]
        public JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:Core.service.livelink.opentext.com", Order=0)]
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public System.Nullable<System.DateTime> GetSessionExpirationDateResult;
        
        public GetSessionExpirationDateResponse()
        {
        }
        
        public GetSessionExpirationDateResponse(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication, System.Nullable<System.DateTime> GetSessionExpirationDateResult)
        {
            this.OTAuthentication = OTAuthentication;
            this.GetSessionExpirationDateResult = GetSessionExpirationDateResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="ImpersonateApplication", WrapperNamespace="urn:Core.service.livelink.opentext.com", IsWrapped=true)]
    public partial class ImpersonateApplicationRequest
    {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="urn:api.ecm.opentext.com")]
        public JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:Core.service.livelink.opentext.com", Order=0)]
        public string applicationID;
        
        public ImpersonateApplicationRequest()
        {
        }
        
        public ImpersonateApplicationRequest(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication, string applicationID)
        {
            this.OTAuthentication = OTAuthentication;
            this.applicationID = applicationID;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="ImpersonateApplicationResponse", WrapperNamespace="urn:Core.service.livelink.opentext.com", IsWrapped=true)]
    public partial class ImpersonateApplicationResponse
    {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="urn:api.ecm.opentext.com")]
        public JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:Core.service.livelink.opentext.com", Order=0)]
        public string ImpersonateApplicationResult;
        
        public ImpersonateApplicationResponse()
        {
        }
        
        public ImpersonateApplicationResponse(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication, string ImpersonateApplicationResult)
        {
            this.OTAuthentication = OTAuthentication;
            this.ImpersonateApplicationResult = ImpersonateApplicationResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="ImpersonateUser", WrapperNamespace="urn:Core.service.livelink.opentext.com", IsWrapped=true)]
    public partial class ImpersonateUserRequest
    {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="urn:api.ecm.opentext.com")]
        public JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:Core.service.livelink.opentext.com", Order=0)]
        public string userName;
        
        public ImpersonateUserRequest()
        {
        }
        
        public ImpersonateUserRequest(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication, string userName)
        {
            this.OTAuthentication = OTAuthentication;
            this.userName = userName;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="ImpersonateUserResponse", WrapperNamespace="urn:Core.service.livelink.opentext.com", IsWrapped=true)]
    public partial class ImpersonateUserResponse
    {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="urn:api.ecm.opentext.com")]
        public JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:Core.service.livelink.opentext.com", Order=0)]
        public string ImpersonateUserResult;
        
        public ImpersonateUserResponse()
        {
        }
        
        public ImpersonateUserResponse(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication, string ImpersonateUserResult)
        {
            this.OTAuthentication = OTAuthentication;
            this.ImpersonateUserResult = ImpersonateUserResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="RefreshToken", WrapperNamespace="urn:Core.service.livelink.opentext.com", IsWrapped=true)]
    public partial class RefreshTokenRequest
    {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="urn:api.ecm.opentext.com")]
        public JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication;
        
        public RefreshTokenRequest()
        {
        }
        
        public RefreshTokenRequest(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication)
        {
            this.OTAuthentication = OTAuthentication;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="RefreshTokenResponse", WrapperNamespace="urn:Core.service.livelink.opentext.com", IsWrapped=true)]
    public partial class RefreshTokenResponse
    {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="urn:api.ecm.opentext.com")]
        public JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="urn:Core.service.livelink.opentext.com", Order=0)]
        public string RefreshTokenResult;
        
        public RefreshTokenResponse()
        {
        }
        
        public RefreshTokenResponse(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication, string RefreshTokenResult)
        {
            this.OTAuthentication = OTAuthentication;
            this.RefreshTokenResult = RefreshTokenResult;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    public interface AuthenticationChannel : JCS.Argon.Services.Soap.Opentext.Authentication, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    public partial class AuthenticationClient : System.ServiceModel.ClientBase<JCS.Argon.Services.Soap.Opentext.Authentication>, JCS.Argon.Services.Soap.Opentext.Authentication
    {
        
        /// <summary>
        /// Implement this partial method to configure the service endpoint.
        /// </summary>
        /// <param name="serviceEndpoint">The endpoint to configure</param>
        /// <param name="clientCredentials">The client credentials</param>
        static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials);
        
        public AuthenticationClient() : 
                base(AuthenticationClient.GetDefaultBinding(), AuthenticationClient.GetDefaultEndpointAddress())
        {
            this.Endpoint.Name = EndpointConfiguration.BasicHttpBinding_Authentication.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public AuthenticationClient(EndpointConfiguration endpointConfiguration) : 
                base(AuthenticationClient.GetBindingForEndpoint(endpointConfiguration), AuthenticationClient.GetEndpointAddress(endpointConfiguration))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public AuthenticationClient(EndpointConfiguration endpointConfiguration, string remoteAddress) : 
                base(AuthenticationClient.GetBindingForEndpoint(endpointConfiguration), new System.ServiceModel.EndpointAddress(remoteAddress))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public AuthenticationClient(EndpointConfiguration endpointConfiguration, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(AuthenticationClient.GetBindingForEndpoint(endpointConfiguration), remoteAddress)
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public AuthenticationClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.AuthenticateApplicationResponse> JCS.Argon.Services.Soap.Opentext.Authentication.AuthenticateApplicationAsync(JCS.Argon.Services.Soap.Opentext.AuthenticateApplicationRequest request)
        {
            return base.Channel.AuthenticateApplicationAsync(request);
        }
        
        public System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.AuthenticateApplicationResponse> AuthenticateApplicationAsync(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication, string applicationID, string password)
        {
            JCS.Argon.Services.Soap.Opentext.AuthenticateApplicationRequest inValue = new JCS.Argon.Services.Soap.Opentext.AuthenticateApplicationRequest();
            inValue.OTAuthentication = OTAuthentication;
            inValue.applicationID = applicationID;
            inValue.password = password;
            return ((JCS.Argon.Services.Soap.Opentext.Authentication)(this)).AuthenticateApplicationAsync(inValue);
        }
        
        public System.Threading.Tasks.Task<string> AuthenticateUserAsync(string userName, string userPassword)
        {
            return base.Channel.AuthenticateUserAsync(userName, userPassword);
        }
        
        public System.Threading.Tasks.Task<string> AuthenticateUserWithApplicationTokenAsync(string userName, string userPassword, string applicationToken)
        {
            return base.Channel.AuthenticateUserWithApplicationTokenAsync(userName, userPassword, applicationToken);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.CombineApplicationTokenResponse> JCS.Argon.Services.Soap.Opentext.Authentication.CombineApplicationTokenAsync(JCS.Argon.Services.Soap.Opentext.CombineApplicationTokenRequest request)
        {
            return base.Channel.CombineApplicationTokenAsync(request);
        }
        
        public System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.CombineApplicationTokenResponse> CombineApplicationTokenAsync(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication, string applicationToken)
        {
            JCS.Argon.Services.Soap.Opentext.CombineApplicationTokenRequest inValue = new JCS.Argon.Services.Soap.Opentext.CombineApplicationTokenRequest();
            inValue.OTAuthentication = OTAuthentication;
            inValue.applicationToken = applicationToken;
            return ((JCS.Argon.Services.Soap.Opentext.Authentication)(this)).CombineApplicationTokenAsync(inValue);
        }
        
        public System.Threading.Tasks.Task<string> GetOTDSResourceIDAsync()
        {
            return base.Channel.GetOTDSResourceIDAsync();
        }
        
        public System.Threading.Tasks.Task<string> GetOTDSServerAsync()
        {
            return base.Channel.GetOTDSServerAsync();
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.GetSessionExpirationDateResponse> JCS.Argon.Services.Soap.Opentext.Authentication.GetSessionExpirationDateAsync(JCS.Argon.Services.Soap.Opentext.GetSessionExpirationDateRequest request)
        {
            return base.Channel.GetSessionExpirationDateAsync(request);
        }
        
        public System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.GetSessionExpirationDateResponse> GetSessionExpirationDateAsync(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication)
        {
            JCS.Argon.Services.Soap.Opentext.GetSessionExpirationDateRequest inValue = new JCS.Argon.Services.Soap.Opentext.GetSessionExpirationDateRequest();
            inValue.OTAuthentication = OTAuthentication;
            return ((JCS.Argon.Services.Soap.Opentext.Authentication)(this)).GetSessionExpirationDateAsync(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.ImpersonateApplicationResponse> JCS.Argon.Services.Soap.Opentext.Authentication.ImpersonateApplicationAsync(JCS.Argon.Services.Soap.Opentext.ImpersonateApplicationRequest request)
        {
            return base.Channel.ImpersonateApplicationAsync(request);
        }
        
        public System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.ImpersonateApplicationResponse> ImpersonateApplicationAsync(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication, string applicationID)
        {
            JCS.Argon.Services.Soap.Opentext.ImpersonateApplicationRequest inValue = new JCS.Argon.Services.Soap.Opentext.ImpersonateApplicationRequest();
            inValue.OTAuthentication = OTAuthentication;
            inValue.applicationID = applicationID;
            return ((JCS.Argon.Services.Soap.Opentext.Authentication)(this)).ImpersonateApplicationAsync(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.ImpersonateUserResponse> JCS.Argon.Services.Soap.Opentext.Authentication.ImpersonateUserAsync(JCS.Argon.Services.Soap.Opentext.ImpersonateUserRequest request)
        {
            return base.Channel.ImpersonateUserAsync(request);
        }
        
        public System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.ImpersonateUserResponse> ImpersonateUserAsync(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication, string userName)
        {
            JCS.Argon.Services.Soap.Opentext.ImpersonateUserRequest inValue = new JCS.Argon.Services.Soap.Opentext.ImpersonateUserRequest();
            inValue.OTAuthentication = OTAuthentication;
            inValue.userName = userName;
            return ((JCS.Argon.Services.Soap.Opentext.Authentication)(this)).ImpersonateUserAsync(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.RefreshTokenResponse> JCS.Argon.Services.Soap.Opentext.Authentication.RefreshTokenAsync(JCS.Argon.Services.Soap.Opentext.RefreshTokenRequest request)
        {
            return base.Channel.RefreshTokenAsync(request);
        }
        
        public System.Threading.Tasks.Task<JCS.Argon.Services.Soap.Opentext.RefreshTokenResponse> RefreshTokenAsync(JCS.Argon.Services.Soap.Opentext.OTAuthentication OTAuthentication)
        {
            JCS.Argon.Services.Soap.Opentext.RefreshTokenRequest inValue = new JCS.Argon.Services.Soap.Opentext.RefreshTokenRequest();
            inValue.OTAuthentication = OTAuthentication;
            return ((JCS.Argon.Services.Soap.Opentext.Authentication)(this)).RefreshTokenAsync(inValue);
        }
        
        public System.Threading.Tasks.Task<string> ValidateUserAsync(string capToken)
        {
            return base.Channel.ValidateUserAsync(capToken);
        }
        
        public virtual System.Threading.Tasks.Task OpenAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndOpen));
        }
        
        public virtual System.Threading.Tasks.Task CloseAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginClose(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndClose));
        }
        
        private static System.ServiceModel.Channels.Binding GetBindingForEndpoint(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.BasicHttpBinding_Authentication))
            {
                System.ServiceModel.BasicHttpBinding result = new System.ServiceModel.BasicHttpBinding();
                result.MaxBufferSize = int.MaxValue;
                result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                result.MaxReceivedMessageSize = int.MaxValue;
                result.AllowCookies = true;
                return result;
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
        
        private static System.ServiceModel.EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.BasicHttpBinding_Authentication))
            {
                return new System.ServiceModel.EndpointAddress("http://ryleh/cws/Authentication.svc");
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
        
        private static System.ServiceModel.Channels.Binding GetDefaultBinding()
        {
            return AuthenticationClient.GetBindingForEndpoint(EndpointConfiguration.BasicHttpBinding_Authentication);
        }
        
        private static System.ServiceModel.EndpointAddress GetDefaultEndpointAddress()
        {
            return AuthenticationClient.GetEndpointAddress(EndpointConfiguration.BasicHttpBinding_Authentication);
        }
        
        public enum EndpointConfiguration
        {
            
            BasicHttpBinding_Authentication,
        }
    }
}
