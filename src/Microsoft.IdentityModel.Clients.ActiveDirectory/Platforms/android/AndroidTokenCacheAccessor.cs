﻿//----------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Android.App;
using Android.Content;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Identity.Core.Cache;
using System;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Identity.Core
{
    internal class AndroidTokenCacheAccessor : ITokenCacheAccessor
    {
        private const string AccessTokenSharedPreferenceName = "com.microsoft.identity.client.accessToken";
        private const string RefreshTokenSharedPreferenceName = "com.microsoft.identity.client.refreshToken";
        private const string IdTokenSharedPreferenceName = "com.microsoft.identity.client.idToken";
        private const string AccountSharedPreferenceName = "com.microsoft.identity.client.Account";

        private readonly ISharedPreferences _accessTokenSharedPreference;
        private readonly ISharedPreferences _refreshTokenSharedPreference;
        private readonly ISharedPreferences _idTokenSharedPreference;
        private readonly ISharedPreferences _accountSharedPreference;

        public AndroidTokenCacheAccessor()
        {
            _accessTokenSharedPreference = Application.Context.GetSharedPreferences(AccessTokenSharedPreferenceName,
                    FileCreationMode.Private);
            _refreshTokenSharedPreference = Application.Context.GetSharedPreferences(RefreshTokenSharedPreferenceName,
                    FileCreationMode.Private);
            _idTokenSharedPreference = Application.Context.GetSharedPreferences(IdTokenSharedPreferenceName,
                    FileCreationMode.Private);
            _accountSharedPreference = Application.Context.GetSharedPreferences(AccountSharedPreferenceName,
                FileCreationMode.Private);

            if (_accessTokenSharedPreference == null || _refreshTokenSharedPreference == null
                || _idTokenSharedPreference == null || _accountSharedPreference == null)
            {
                throw AdalExceptionFactory.GetClientException(
                    ErrorCodes.FailedToCreateSharedPreference,
                    "Fail to create SharedPreference");
            }
        }

        public AndroidTokenCacheAccessor(RequestContext requestContext) : this()
        {
        }

        public void SaveAccessToken(MsalAccessTokenCacheItem item)
        {
            ISharedPreferencesEditor editor = _accessTokenSharedPreference.Edit();
            editor.PutString(item.GetKey().ToString(), item.ToJsonString());
            editor.Apply();
        }

        public void SaveRefreshToken(MsalRefreshTokenCacheItem item)
        {
            ISharedPreferencesEditor editor = _refreshTokenSharedPreference.Edit();
            editor.PutString(item.GetKey().ToString(), item.ToJsonString());
            editor.Apply();
        }

        public void SaveIdToken(MsalIdTokenCacheItem item)
        {
            ISharedPreferencesEditor editor = _idTokenSharedPreference.Edit();
            editor.PutString(item.GetKey().ToString(), item.ToJsonString());
            editor.Apply();
        }

        public void SaveAccount(MsalAccountCacheItem item)
        {
            ISharedPreferencesEditor editor = _accountSharedPreference.Edit();
            editor.PutString(item.GetKey().ToString(), item.ToJsonString());
            editor.Apply();
        }

        private void Delete(string key, ISharedPreferencesEditor editor)
        {
            editor.Remove(key);
            editor.Apply();
        }

        private void DeleteAll(ISharedPreferences sharedPreferences)
        {
            var editor = sharedPreferences.Edit();

            editor.Clear();
            editor.Apply();
        }

        public ICollection<MsalAccessTokenCacheItem> GetAllAccessTokens()
        {
            return _accessTokenSharedPreference.All.Values.Cast<string>().Select(MsalAccessTokenCacheItem.FromJsonString).ToList();
        }

        public ICollection<MsalRefreshTokenCacheItem> GetAllRefreshTokens()
        {
            return _refreshTokenSharedPreference.All.Values.Cast<string>().Select(MsalRefreshTokenCacheItem.FromJsonString).ToList();
        }

        public ICollection<MsalIdTokenCacheItem> GetAllIdTokens()
        {
            return _idTokenSharedPreference.All.Values.Cast<string>().Select(MsalIdTokenCacheItem.FromJsonString).ToList();
        }

        public ICollection<MsalAccountCacheItem> GetAllAccounts()
        {
            return _accountSharedPreference.All.Values.Cast<string>().Select(MsalAccountCacheItem.FromJsonString).ToList();
        }

        public void Clear()
        {
            DeleteAll(_accessTokenSharedPreference);
            DeleteAll(_refreshTokenSharedPreference);
            DeleteAll(_idTokenSharedPreference);
            DeleteAll(_accessTokenSharedPreference);
        }

        /// <inheritdoc />
        public int RefreshTokenCount => throw new NotImplementedException();

        /// <inheritdoc />
        public int AccessTokenCount => throw new NotImplementedException();

        /// <inheritdoc />
        public int AccountCount => throw new NotImplementedException();

        /// <inheritdoc />
        public int IdTokenCount => throw new NotImplementedException();

        /// <inheritdoc />
        public void ClearRefreshTokens()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void ClearAccessTokens()
        {
            throw new NotImplementedException();
        }
    }
}
